using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Attributes {
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class VisibleIfAttribute : PropertyAttribute {

        public readonly string property;
        public readonly float value;
        public readonly CompareMode mode;

        public VisibleIfAttribute(string property, float value = 1f, CompareMode mode = CompareMode.Equal) {
            this.property = property;
            this.value = value;
            this.mode = mode;
        }
    }

}
