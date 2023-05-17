using System;
using UnityEditor;

namespace MisterGames.Common.Editor.Attributes.ReadOnly {

    public readonly struct SerializedDependencyProperty {

        public readonly SerializedProperty property;
        public readonly string name;
        public readonly string category;
        public readonly Type type;

        public SerializedDependencyProperty(SerializedProperty property, string name, string category, Type type) {
            this.property = property;
            this.name = name;
            this.category = category;
            this.type = type;
        }
    }

}
