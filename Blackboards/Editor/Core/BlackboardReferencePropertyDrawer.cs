﻿using System;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Editor.Attributes.SubclassSelector;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blackboards.Editor {

    [CustomPropertyDrawer(typeof(BlackboardReference))]
    public sealed class BlackboardReferencePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            if (BlackboardUtils.TryGetBlackboardPropertyData(property, out var data)) {
                var baseType = data.property.type.ToType();
                if (baseType.IsArray) baseType = baseType.GetElementType();

                SubclassSelectorGUI.PropertyField(position, property.FindPropertyRelative("value"), baseType, label, includeChildren: true);
            }
            else {
                BlackboardUtils.NullPropertyField(position, label);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return BlackboardUtils.TryGetBlackboardPropertyData(property, out _)
                ? SubclassSelectorGUI.GetPropertyHeight(property.FindPropertyRelative("value"), label, includeChildren: true)
                : BlackboardUtils.GetNullPropertyHeight();
        }
    }

}
