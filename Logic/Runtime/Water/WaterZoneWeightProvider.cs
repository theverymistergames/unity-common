using MisterGames.Common.Jobs;
using MisterGames.Common.Volumes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(WaterZone))]
    public sealed class WaterZoneWeightProvider : PositionWeightProvider {

        [SerializeField] private WaterZone _waterZone;
        
        private readonly struct VolumeData {
            
            public readonly float3 position;
            public readonly quaternion rotation;
            public readonly float3 size;
            public readonly int volumeId;
            
            public VolumeData(float3 position, quaternion rotation, float3 size, int volumeId) {
                this.position = position;
                this.rotation = rotation;
                this.size = size;
                this.volumeId = volumeId;
            }
        }
        
        private NativeArray<VolumeData> _volumeDataArray;
        private int _volumeDataArrayCreationFrame;
        private int _volumeCount;

        private void OnDestroy() {
            if (_volumeDataArray.IsCreated) _volumeDataArray.Dispose();
        }

        public override WeightData GetWeight(Vector3 position) {
            var positionArray = new NativeArray<float3>(2, Allocator.TempJob);
            var resultArray = new NativeArray<WeightData>(2, Allocator.TempJob);

            positionArray[0] = position;
            
            GetWeight(positionArray, resultArray, 1);
            
            var result = resultArray[0];

            positionArray.Dispose();
            resultArray.Dispose();
            
            return result;
        }

        public override void GetWeight(NativeArray<float3> positions, NativeArray<WeightData> results, int count) {
            if (count <= 0) return;
            
            UpdateVolumeDataArray();

            int batchCount = JobExt.BatchFor(count);
            
            if (_volumeCount <= 0) {
                var writeZeroWeightJob = new WriteConstWeightJob {
                    weight = 0f,
                    defaultVolumeId = GetHashCode(),
                    results = results,
                };
                
                writeZeroWeightJob.Schedule(count, batchCount).Complete();
                return;
            }
            
            var weightArray = new NativeArray<WeightData>(_volumeCount * count, Allocator.TempJob);
            
            var calculateWeightJob = new CalculateWeightJob {
                volumeDataArray = _volumeDataArray,
                positions = positions,
                weightArray = weightArray,
            };

            var calculateMaxWeightJob = new CalculateMaxWeightJob {
                weightArray = weightArray,
                defaultVolumeId = GetHashCode(),
                volumeCount = _volumeCount,
                results = results,
            };

            var calculateWeightJobHandle = calculateWeightJob.Schedule(_volumeCount * count, _volumeCount);
            calculateMaxWeightJob.Schedule(count, batchCount, calculateWeightJobHandle).Complete();

            weightArray.Dispose();
        }

        private void UpdateVolumeDataArray() {
            int frame = Time.frameCount;
            if (_volumeDataArrayCreationFrame >= frame && _volumeDataArray.IsCreated) return;

            var volumes = _waterZone.Volumes;
            _volumeCount = volumes.Count;
            _volumeDataArrayCreationFrame = frame;
            
            if (!_volumeDataArray.IsCreated || _volumeDataArray.Length < _volumeCount) {
                if (_volumeDataArray.IsCreated) _volumeDataArray.Dispose();
                _volumeDataArray = new NativeArray<VolumeData>(_volumeCount, Allocator.Persistent);
            }
            
            int index = 0;
            
            foreach (var volume in volumes) {
                volume.GetBox(out var position, out var rotation, out var size);
                _volumeDataArray[index++] = new VolumeData(position, rotation, size, _waterZone.GetVolumeId(volume));
            }
        }
        
        [BurstCompile]
        private struct WriteConstWeightJob : IJobParallelFor {
            
            [ReadOnly] public int defaultVolumeId;
            [ReadOnly] public float weight;
            
            [WriteOnly] public NativeArray<WeightData> results;

            public void Execute(int index) {
                results[index] = new WeightData(weight, defaultVolumeId);
            }
        }
        
        [BurstCompile]
        private struct CalculateWeightJob : IJobParallelForBatch {
            
            [ReadOnly] public NativeArray<VolumeData> volumeDataArray;
            [ReadOnly] public NativeArray<float3> positions;
            
            [WriteOnly] public NativeArray<WeightData> weightArray;
            
            public void Execute(int startIndex, int count) {
                var position = positions[startIndex / count];
                
                for (int i = 0; i < count; i++) {
                    var volumeData = volumeDataArray[i];
                
                    var localPoint = math.mul(math.inverse(volumeData.rotation), position - volumeData.position);
                    var halfSize = volumeData.size * 0.5f;

                    if (localPoint.x < -halfSize.x || localPoint.x > halfSize.x ||
                        localPoint.y < -halfSize.y || localPoint.y > halfSize.y ||
                        localPoint.z < -halfSize.z || localPoint.z > halfSize.z) 
                    {
                        weightArray[startIndex + i] = new WeightData(0f, volumeData.volumeId);
                        continue;
                    }
                
                    weightArray[startIndex + i] = new WeightData(1f, volumeData.volumeId);
                }
            }
        }
        
        [BurstCompile]
        private struct CalculateMaxWeightJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<WeightData> weightArray;
            [ReadOnly] public int defaultVolumeId;
            [ReadOnly] public int volumeCount;
            
            [WriteOnly] public NativeArray<WeightData> results;
            
            public void Execute(int index) {
                int from = index * volumeCount;
                int to = from + volumeCount;
                
                for (int i = from; i < to; i++) {
                    var data = weightArray[i];
                    if (data.weight <= 0f) continue;

                    results[index] = data;
                    return;
                }

                results[index] = new WeightData(0f, defaultVolumeId);
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _waterZone = GetComponent<WaterZone>();
        }
#endif
    }
    
}