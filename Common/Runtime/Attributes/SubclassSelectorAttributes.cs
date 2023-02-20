using System;
using UnityEngine;

namespace MisterGames.Common.Attributes {

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SubclassSelectorAttribute : PropertyAttribute {

        public readonly string explicitSerializedTypePath;

        public SubclassSelectorAttribute() {
            explicitSerializedTypePath = null;
        }

        public SubclassSelectorAttribute(string explicitSerializedTypePath) {
            this.explicitSerializedTypePath = explicitSerializedTypePath;
        }
    }

}
