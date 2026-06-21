using System;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class LabelFilterAttribute : PropertyAttribute {
        
        public readonly string path;
        public readonly bool ignoreValueType;

        public LabelFilterAttribute(string path = null, bool ignoreValueType = false) {
            this.path = path;
            this.ignoreValueType = ignoreValueType;
        }
    }
    
}