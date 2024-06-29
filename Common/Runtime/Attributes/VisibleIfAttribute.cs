using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Attributes {
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class VisibleIfAttribute : PropertyAttribute {

        public readonly string property;
        public readonly int value;
        public readonly CompareMode mode;

        public VisibleIfAttribute(string property, int value = 1, CompareMode mode = CompareMode.Equal) {
            this.property = property;
            this.value = value;
            this.mode = mode;
        }
    }

}
