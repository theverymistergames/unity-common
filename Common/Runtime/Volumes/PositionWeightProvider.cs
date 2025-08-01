using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public abstract class PositionWeightProvider : MonoBehaviour, IPositionWeightProvider {
        
        public abstract WeightData GetWeight(Vector3 position);
        
        public abstract void GetWeight(NativeArray<float3> positions, NativeArray<WeightData> results, int count);
    }
    
}