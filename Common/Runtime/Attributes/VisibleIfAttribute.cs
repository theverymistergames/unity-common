using System;
using UnityEngine;

namespace MisterGames.Common.Attributes {

    [AttributeUsage(AttributeTargets.Field)]
    public class VisibleIfAttribute : PropertyAttribute {

        public readonly string boolPropertyName;

        public VisibleIfAttribute(string boolPropertyName) {
            this.boolPropertyName = boolPropertyName;
        }
    }

}
