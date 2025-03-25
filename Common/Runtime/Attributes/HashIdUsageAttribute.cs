using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Attributes {
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HashIdUsageAttribute : PropertyAttribute {
        
        public readonly HashMethod hashMethod;

        public HashIdUsageAttribute(HashMethod method) {
            hashMethod = method;
        }
    }

}
