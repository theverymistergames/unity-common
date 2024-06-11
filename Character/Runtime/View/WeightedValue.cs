using System;
using UnityEngine;

namespace MisterGames.Character.View {

    [Serializable]
    public struct WeightedValue<T> {
        
        [Range(0f, 1f)] public float weight;
        public T value;
        
        public WeightedValue(float weight, T value) {
            this.weight = weight;
            this.value = value;
        }
    }
    
    
}
