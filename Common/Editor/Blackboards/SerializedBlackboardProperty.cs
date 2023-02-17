using MisterGames.Common.Data;
using UnityEditor;

namespace MisterGames.Common.Editor.Blackboards {

    public readonly struct SerializedBlackboardProperty {

        public readonly BlackboardProperty blackboardProperty;
        public readonly SerializedProperty serializedProperty;

        public SerializedBlackboardProperty(BlackboardProperty blackboardProperty, SerializedProperty serializedProperty) {
            this.blackboardProperty = blackboardProperty;
            this.serializedProperty = serializedProperty;
        }
    }

}
