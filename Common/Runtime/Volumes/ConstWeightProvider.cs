using MisterGames.Common.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public sealed class ConstWeightProvider : PositionWeightProvider {

        [SerializeField] [Range(0f, 1f)] private float _weight = 1f;
        
        public override WeightData GetWeight(Vector3 position) {
            return new WeightData(_weight, volumeId: GetHashCode(), position);
        }

        public override void GetWeight(NativeArray<float3> positions, NativeArray<WeightData> results, int count) {
            if (count <= 0) return;
            
            var job = new CalculateWeightJob {
                positions = positions,
                weight = _weight,
                volumeId = GetHashCode(),
                results = results
            };
            
            job.Schedule(count, JobExt.BatchFor(count)).Complete();
        }

        [BurstCompile]
        private struct CalculateWeightJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<float3> positions;
            [ReadOnly] public int volumeId;
            [ReadOnly] public float weight;
            [WriteOnly] public NativeArray<WeightData> results;

            public void Execute(int index) {
                results[index] = new WeightData(weight, volumeId, positions[index]);
            }
        }
    }
    
}