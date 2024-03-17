using System;
using UnityEngine;

namespace MisterGames.Common.Attributes {
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinMaxSliderAttribute : PropertyAttribute {
        
        public readonly float min;
        public readonly float max;
        public readonly bool show;

        public MinMaxSliderAttribute(float min, float max, bool show = false) {
            this.min = min;
            this.max = max;
            this.show = show;
        }
        
        public MinMaxSliderAttribute(int min, int max, bool show = false) {
            this.min = min;
            this.max = max;
            this.show = show;
        }
    }
}