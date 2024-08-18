using System;
using UnityEngine;

namespace MisterGames.Common.Attributes {
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinMaxSliderAttribute : PropertyAttribute {
        
        public readonly float min;
        public readonly float max;

        public MinMaxSliderAttribute(float min, float max) {
            this.min = min;
            this.max = max;
        }
        
        public MinMaxSliderAttribute(int min, int max) {
            this.min = min;
            this.max = max;
        }
    }
}