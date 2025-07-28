using System;
using MisterGames.Common.Types;
using UnityEngine;

namespace MisterGames.Common.Attributes {
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TypeFilterAttribute : PropertyAttribute {
        
        public readonly string propertyName;
        public readonly TypeFilterMode mode;

        public TypeFilterAttribute(string propertyName, TypeFilterMode mode) {
            this.propertyName = propertyName;
            this.mode = mode;
        }
    }
    
}