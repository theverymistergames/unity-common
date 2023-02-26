﻿using MisterGames.Blackboards.Core;
using MisterGames.Common.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blackboards.Editor {

    [CustomPropertyDrawer(typeof(BlackboardReference))]
    public sealed class BlackboardReferencePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            if (BlackboardUtils.TryGetBlackboardProperty(property, out var blackboardProperty) && blackboardProperty.type != null) {
                SubclassSelectorGUI.PropertyField(position, property.FindPropertyRelative("value"), blackboardProperty.type, label);
            }
            else {
                BlackboardUtils.NullPropertyField(position, label);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (BlackboardUtils.TryGetBlackboardProperty(property, out var blackboardProperty) && blackboardProperty.type != null) {
                return SubclassSelectorGUI.GetPropertyHeight(property.FindPropertyRelative("value"), true);
            }

            return BlackboardUtils.GetNullPropertyHeight();
        }
    }

}
