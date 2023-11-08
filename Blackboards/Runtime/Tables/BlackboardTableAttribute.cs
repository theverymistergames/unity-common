using System;
using UnityEngine;

namespace MisterGames.Blackboards.Tables {

    [AttributeUsage(validOn: AttributeTargets.Class)]
    public class BlackboardTableAttribute : PropertyAttribute {

        public readonly Type elementType;

        public BlackboardTableAttribute(Type elementType) {
            this.elementType = elementType;
        }
    }

}
