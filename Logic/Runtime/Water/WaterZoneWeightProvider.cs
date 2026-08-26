using MisterGames.Common.Jobs;
using MisterGames.Common.Volumes;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
            UpdateVolumeDataArray();

            // Weight is binary here, and a point inside a volume has zero distance to it,
            // so the first containing volume is the result. No jobs needed for a single position.
            for (int i = 0; i < _volumeCount; i++) {
                var volumeData = _volumeDataArray[i];
                if (Contains(volumeData, position)) return new WeightData(1f, volumeData.volumeId, position);
            }

            return new WeightData(0f, GetHashCode(), position);
        }

        public override JobHandle GetWeight(NativeArray<float3> positions, NativeArray<WeightSample> results, int count, JobHandle dependency = default) {
            if (count <= 0) return dependency;

            UpdateVolumeDataArray();

            int batchCount = JobExt.BatchFor(count, MinBatch);

            if (_volumeCount <= 0) {
                var writeZeroWeightJob = new WriteConstResultJob {
                    weight = 0f,
                    defaultVolumeId = GetHashCode(),
                    results = results,
                };

                return writeZeroWeightJob.Schedule(count, batchCount, dependency);
            }

            var weightArray = new NativeArray<WeightSample>(_volumeCount * count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

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

            var calculateWeightJobHandle = calculateWeightJob.Schedule(_volumeCount * count, _volumeCount, dependency);
            var resultJobHandle = calculateMaxWeightJob.Schedule(count, batchCount, calculateWeightJobHandle);

            weightArray.Dispose(resultJobHandle);

            return resultJobHandle;
        }

        private static bool Contains(VolumeData volumeData, float3 position) {
            var localPoint = math.mul(math.inverse(volumeData.rotation), position - volumeData.position);
            var halfSize = volumeData.size * 0.5f;

            return localPoint.x >= -halfSize.x && localPoint.x <= halfSize.x &&
                   localPoint.y >= -halfSize.y && localPoint.y <= halfSize.y &&
                   localPoint.z >= -halfSize.z && localPoint.z <= halfSize.z;
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
        private struct WriteConstResultJob : IJobParallelFor {

            [ReadOnly] public int defaultVolumeId;
            [ReadOnly] public float weight;

            // Results can be a sub array of a buffer shared between volumes:
            // each volume writes into its own disjoint range, so aliasing check is not applicable.
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly] public NativeArray<WeightSample> results;

            public void Execute(int index) {
                results[index] = new WeightSample(weight, defaultVolumeId);
            }
        }

        [BurstCompile]
        private struct CalculateWeightJob : IJobParallelForBatch {

            [ReadOnly] public NativeArray<VolumeData> volumeDataArray;
            [ReadOnly] public NativeArray<float3> positions;
            [WriteOnly] public NativeArray<WeightSample> weightArray;

            public void Execute(int startIndex, int count) {
                var position = positions[startIndex / count];

                for (int i = 0; i < count; i++) {
                    var volumeData = volumeDataArray[i];

                    var localPoint = math.mul(math.inverse(volumeData.rotation), position - volumeData.position);
                    var halfSize = volumeData.size * 0.5f;

                    bool inside = localPoint.x >= -halfSize.x && localPoint.x <= halfSize.x &&
                                  localPoint.y >= -halfSize.y && localPoint.y <= halfSize.y &&
                                  localPoint.z >= -halfSize.z && localPoint.z <= halfSize.z;

                    weightArray[startIndex + i] = new WeightSample(inside ? 1f : 0f, volumeData.volumeId);
                }
            }
        }

        [BurstCompile]
        private struct CalculateMaxWeightJob : IJobParallelFor {

            [ReadOnly] public NativeArray<WeightSample> weightArray;
            [ReadOnly] public int defaultVolumeId;
            [ReadOnly] public int volumeCount;

            // Results can be a sub array of a buffer shared between volumes:
            // each volume writes into its own disjoint range, so aliasing check is not applicable.
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly] public NativeArray<WeightSample> results;

            public void Execute(int index) {
                int from = index * volumeCount;
                int to = from + volumeCount;

                // Weight is binary and a contained point has zero distance to its volume,
                // so the first volume with non zero weight is the closest one.
                for (int i = from; i < to; i++) {
                    var data = weightArray[i];
                    if (data.weight <= 0f) continue;

                    results[index] = data;
                    return;
                }

                results[index] = new WeightSample(0f, defaultVolumeId);
            }
        }

#if UNITY_EDITOR
        private void Reset() {
            _waterZone = GetComponent<WaterZone>();
        }
#endif
    }

}
