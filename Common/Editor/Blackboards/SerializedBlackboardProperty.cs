using MisterGames.Common.Data;
using UnityEditor;

namespace MisterGames.Common.Editor.Blackboards {

    public readonly struct SerializedBlackboardProperty {

        public readonly int hash;
        public readonly BlackboardProperty blackboardProperty;
        public readonly SerializedProperty serializedProperty;

        public SerializedBlackboardProperty(int hash, BlackboardProperty blackboardProperty, SerializedProperty serializedProperty) {
            this.hash = hash;
            this.blackboardProperty = blackboardProperty;
            this.serializedProperty = serializedProperty;
        }
    }

}
