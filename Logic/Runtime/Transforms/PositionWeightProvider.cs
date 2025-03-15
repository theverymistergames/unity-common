using UnityEngine;

namespace MisterGames.Logic.Transforms {
    
    public abstract class PositionWeightProvider : MonoBehaviour, IPositionWeightProvider {
        
        public abstract float GetWeight(Vector3 position);
        
    }
    
}