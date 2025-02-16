using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class CustomGravity {
        
        public static readonly CustomGravity Main = new();
        
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

        public bool TryGetGlobalGravity(Vector3 position, out Vector3 gravity) {
            gravity = Vector3.zero;
            float weightSum = 0f;
            
            foreach (var source in _sources) {
                var g = source.GetGravity(position, out float weight);
                float w = Mathf.Abs(weight);
                
                weightSum += w;
                gravity += g * w;
            }

            if (weightSum > 0f) {
                gravity /= weightSum;
                return true;
            }
            
            return false;
        }
        
        public Vector3 GetGlobalGravity(Vector3 position, Vector3 defaultGravity = default) {
            return TryGetGlobalGravity(position, out var gravity) ? gravity : defaultGravity;
        }
    }
    
}