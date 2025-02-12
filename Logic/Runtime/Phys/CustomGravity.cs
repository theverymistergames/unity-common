using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public static class CustomGravity {
    
        public enum Mode {
            Physics,
            CustomGlobal,
            CustomLocal,
        }
        
        public interface IGravitySource {
            Vector3 GetGravity(Vector3 position, out float weight);
        }
        
        private static readonly HashSet<IGravitySource> _sources = new();

        public static void RegisterGravitySource(IGravitySource source) {
            _sources.Add(source);
        }

        public static void UnregisterGravitySource(IGravitySource source) {
            _sources.Remove(source);
        }

        public static Vector3 GetGlobalGravity(Vector3 position) {
            if (_sources.Count == 0) return Vector3.zero;
            
            var gravity = Vector3.zero;
            float weightSum = 0f;
            
            foreach (var source in _sources) {
                var g = source.GetGravity(position, out float weight);
                
                weightSum += Mathf.Abs(weight);
                gravity += g * weight;
            }
            
            return weightSum > 0f ? gravity / weightSum : Vector3.zero;
        }
    }
    
}