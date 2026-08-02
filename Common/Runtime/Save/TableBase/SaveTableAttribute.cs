using System;
using UnityEngine;

namespace MisterGames.Common.Save.Tables {
    
    [AttributeUsage(validOn: AttributeTargets.Class)]
    public class SaveTableAttribute : PropertyAttribute {

        public readonly Type keyType;
        public readonly Type valueType;

        public SaveTableAttribute(Type key, Type value = null) {
            keyType = key;
            valueType = value;
        }
    }
    
}