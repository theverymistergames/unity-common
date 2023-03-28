using MisterGames.Blackboards.Core;
using UnityEditor;

namespace MisterGames.Blackboards.Editor {

    public readonly struct SerializedBlackboardProperty {

        public readonly BlackboardProperty blackboardProperty;
        public readonly SerializedProperty serializedProperty;

        public SerializedBlackboardProperty(BlackboardProperty blackboardProperty, SerializedProperty serializedProperty) {
            this.blackboardProperty = blackboardProperty;
            this.serializedProperty = serializedProperty;
        }
    }

}
