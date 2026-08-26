using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public abstract class PositionWeightProvider : MonoBehaviour, IPositionWeightProvider {
        
        protected const int MinBatch = 16;
        
        public abstract WeightData GetWeight(Vector3 position);
        
        public abstract JobHandle GetWeight(NativeArray<float3> positions, NativeArray<WeightSample> results, int count, JobHandle dependency = default);
    }
    
}
