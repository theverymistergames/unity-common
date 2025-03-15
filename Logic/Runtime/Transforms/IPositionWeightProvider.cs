using UnityEngine;

namespace MisterGames.Logic.Transforms {
    
    public interface IPositionWeightProvider {
    
        float GetWeight(Vector3 position);
        
    }
    
}