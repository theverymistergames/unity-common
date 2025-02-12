using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class CustomGravity {
        
        public static readonly CustomGravity Main = new CustomGravity();
        
        private readonly HashSet<IGravitySource> _sources = new();

        public void AddGravitySource(IGravitySource source) {
            _sources.Add(source);
        }

        public void RemoveGravitySource(IGravitySource source) {
            _sources.Remove(source);
        }

        public void ClearSources() {
            _sources.Clear();
        }

        public Vector3 GetGlobalGravity(Vector3 position) {
            if (_sources.Count == 0) return Vector3.zero;
            
            var gravity = Vector3.zero;
            float weightSum = 0f;
            
            foreach (var source in _sources) {
                var g = source.GetGravity(position, out float weight);
                float w = Mathf.Abs(weight);
                
                weightSum += w;
                gravity += g * w;
            }
            
            return weightSum > 0f ? gravity / weightSum : Vector3.zero;
        }
    }
    
}