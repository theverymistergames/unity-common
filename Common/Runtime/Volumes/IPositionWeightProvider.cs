using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public interface IPositionWeightProvider {

        WeightData GetWeight(Vector3 position);
        
        void GetWeight(NativeArray<float3> positions, NativeArray<WeightData> results, int count);
    }
    
}