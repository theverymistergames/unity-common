using UnityEngine;

namespace MisterGames.Common.Collisions {
    
    public static class CollisionUtils {
    
        public static bool Contains(this LayerMask mask, int layer) {
            return mask == (mask | (1 << layer));
        }
        
    }
    
}