using System;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class LabelValueVisibilityAttribute : PropertyAttribute {
        
        public readonly bool lib;
        public readonly bool array;
        
        public LabelValueVisibilityAttribute(bool lib, bool array) {
            this.lib = lib;
            this.array = array;
        }
    }
    
}