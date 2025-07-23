using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public interface IPositionWeightProvider {
    
        float GetWeight(Vector3 position);
        
    }
    
}