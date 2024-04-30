using System;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {
    
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public class SaveTableAttribute : PropertyAttribute {

        public readonly Type elementType;

        public SaveTableAttribute(Type elementType) {
            this.elementType = elementType;
        }
    }
    
}