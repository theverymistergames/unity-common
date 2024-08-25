using UnityEngine;

namespace MisterGames.Common.Maths {
    
    public readonly struct ThrowData {
        
        public readonly Vector3 velocity;
        public readonly float time;
        
        public ThrowData(Vector3 velocity, float time) {
            this.velocity = velocity;
            this.time = time;
        }
    }
    
}