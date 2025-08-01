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

        private readonly struct ProxyData {
            
            public readonly float3 position;
            public readonly quaternion rotation;
            public readonly float3 size;
            public readonly int volumeId;
            
            public ProxyData(float3 position, quaternion rotation, float3 size, int volumeId) {
                this.position = position;
                this.rotation = rotation;
                this.size = size;
                this.volumeId = volumeId;
            }
        }
        
        private NativeArray<ProxyData> _proxyDataArray;
        private int _proxyDataArrayCreationFrame;
        private int _proxyCount;

        private void OnDestroy() {
            if (_proxyDataArray.IsCreated) _proxyDataArray.Dispose();
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
            
            UpdateProxyDataArray();

            if (_proxyCount <= 0) {
                var writeZeroWeightJob = new WriteConstWeightJob {
                    weight = 0f,
                    defaultVolumeId = GetHashCode(),
                    results = results,
                };
                
                writeZeroWeightJob.Schedule(count, UnityJobsExt.BatchCount(count)).Complete();
                return;
            }
            
            var weightArray = new NativeArray<WeightData>(_proxyCount * count, Allocator.TempJob);
            
            var calculateWeightJob = new CalculateWeightJob {
                proxyDataArray = _proxyDataArray,
                positions = positions,
                proxyCount = _proxyCount,
                weightArray = weightArray,
            };

            var calculateMaxWeightJob = new CalculateMaxWeightJob {
                weightArray = weightArray,
                defaultVolumeId = GetHashCode(),
                proxyCount = _proxyCount,
                results = results,
            };
            
            var calculateWeightJobHandle = calculateWeightJob.Schedule(_proxyCount * count, UnityJobsExt.BatchCount(_proxyCount * count));
            calculateMaxWeightJob.Schedule(count, UnityJobsExt.BatchCount(count), calculateWeightJobHandle).Complete();

            weightArray.Dispose();
        }

        private void UpdateProxyDataArray() {
            int frame = Time.frameCount;
            if (_proxyDataArrayCreationFrame >= frame && _proxyDataArray.IsCreated) return;

            var proxySet = _waterZone.WaterProxySet;
            _proxyCount = proxySet.Count;
            _proxyDataArrayCreationFrame = frame;
            
            if (!_proxyDataArray.IsCreated || _proxyDataArray.Length < _proxyCount) {
                if (_proxyDataArray.IsCreated) _proxyDataArray.Dispose();
                _proxyDataArray = new NativeArray<ProxyData>(_proxyCount, Allocator.Persistent);
            }
            
            int index = 0;
            
            foreach (var proxy in proxySet) {
                proxy.GetBox(out var position, out var rotation, out var size);
                _proxyDataArray[index++] = new ProxyData(position, rotation, size, _waterZone.GetProxyVolumeId(proxy));
            }
        }
        
        [BurstCompile]
        private struct WriteConstWeightJob : IJobParallelFor {
            
            [ReadOnly] public int defaultVolumeId;
            [ReadOnly] public float weight;
            public NativeArray<WeightData> results;

            public void Execute(int index) {
                results[index] = new WeightData(weight, defaultVolumeId);
            }
        }
        
        [BurstCompile]
        private struct CalculateWeightJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<ProxyData> proxyDataArray;
            [ReadOnly] public NativeArray<float3> positions;
            [ReadOnly] public int proxyCount;
            
            public NativeArray<WeightData> weightArray;
            
            public void Execute(int index) {
                var proxyData = proxyDataArray[index % proxyCount];
                var position = positions[(int) math.floor((float) index / proxyCount)];
                
                var localPoint = math.mul(math.inverse(proxyData.rotation), position - proxyData.position);
                var halfSize = proxyData.size * 0.5f;

                if (localPoint.x < -halfSize.x || localPoint.x > halfSize.x ||
                    localPoint.y < -halfSize.y || localPoint.y > halfSize.y ||
                    localPoint.z < -halfSize.z || localPoint.z > halfSize.z) 
                {
                    weightArray[index] = new WeightData(0f, proxyData.volumeId);
                    return;
                }
                
                weightArray[index] = new WeightData(1f, proxyData.volumeId);
            }
        }
        
        [BurstCompile]
        private struct CalculateMaxWeightJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<WeightData> weightArray;
            [ReadOnly] public int defaultVolumeId;
            [ReadOnly] public int proxyCount;
            
            public NativeArray<WeightData> results;
            
            public void Execute(int index) {
                int from = index * proxyCount;
                int to = from + proxyCount;
                
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