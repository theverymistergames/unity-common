using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public abstract class PositionWeightProvider : MonoBehaviour, IPositionWeightProvider {
        
        public abstract float GetWeight(Vector3 position, out int volumeId);
        
    }
    
}