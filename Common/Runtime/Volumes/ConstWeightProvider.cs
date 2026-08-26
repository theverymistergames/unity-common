using MisterGames.Common.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public sealed class ConstWeightProvider : PositionWeightProvider {

        [SerializeField] [Range(0f, 1f)] private float _weight = 1f;
        
        public override WeightData GetWeight(Vector3 position) {
            return new WeightData(_weight, volumeId: GetHashCode(), position);
        }

        public override JobHandle GetWeight(NativeArray<float3> positions, NativeArray<WeightSample> results, int count, JobHandle dependency = default) {
            if (count <= 0) return dependency;
            
            var job = new CalculateWeightJob {
                weight = _weight,
                volumeId = GetHashCode(),
                results = results
            };
            
            return job.Schedule(count, JobExt.BatchFor(count, MinBatch), dependency);
        }

        [BurstCompile]
        private struct CalculateWeightJob : IJobParallelFor {
            
            [ReadOnly] public int volumeId;
            [ReadOnly] public float weight;
            
            // Results can be a sub array of a bigger buffer shared between volumes:
            // each volume writes into its own disjoint range, so aliasing check is not applicable.
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly] public NativeArray<WeightSample> results;

            public void Execute(int index) {
                results[index] = new WeightSample(weight, volumeId);
            }
        }
    }
    
}
