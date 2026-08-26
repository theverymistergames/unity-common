using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public interface IPositionWeightProvider {

        WeightData GetWeight(Vector3 position);
        
        /// <summary>
        /// Schedules weight calculation for <paramref name="count"/> positions and returns the job handle
        /// without completing it, so that caller can batch several providers into one sync point.
        /// </summary>
        JobHandle GetWeight(NativeArray<float3> positions, NativeArray<WeightSample> results, int count, JobHandle dependency = default);
    }
    
}
