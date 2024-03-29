﻿using System;
using System.Reflection;
using MisterGames.Blackboards.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Editor {

    [CustomPropertyDrawer(typeof(BlackboardValue<>))]
    public sealed class BlackboardValuePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            if (BlackboardUtils.TryGetBlackboardPropertyData(property, out var data)) {
                var type = data.property.type.ToType();
                if (type.IsArray) type = type.GetElementType();

                TypedField(position, property.FindPropertyRelative("value"), type, label);
            }
            else {
                BlackboardUtils.NullPropertyField(position, label);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return BlackboardUtils.TryGetBlackboardPropertyData(property, out var data)
                ? GetTypedFieldHeight(property, label, data.property.type.ToType())
                : BlackboardUtils.GetNullPropertyHeight();
        }

        private static void TypedField(Rect position, SerializedProperty property, Type type, GUIContent label) {
            if (typeof(Object).IsAssignableFrom(type)) {
                EditorGUI.ObjectField(position, property, type, label);
                return;
            }

            if (type.IsEnum) {
                var currentEnumValue = Enum.ToObject(type, property.ulongValue) as Enum;

                var result = type.GetCustomAttribute<FlagsAttribute>(false) != null
                    ? EditorGUI.EnumFlagsField(position, label, currentEnumValue)
                    : EditorGUI.EnumPopup(position, label, currentEnumValue);

                if (!Equals(result, currentEnumValue)) {
                    property.ulongValue = Convert.ToUInt64(result);

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }

                return;
            }

            EditorGUI.PropertyField(position, property, label, includeChildren: true);
        }

        private static float GetTypedFieldHeight(SerializedProperty property, GUIContent label, Type type) {
            return typeof(Object).IsAssignableFrom(type) ? EditorGUI.GetPropertyHeight(property)
                : type.IsEnum ? EditorGUIUtility.singleLineHeight
                : EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
        }
    }

}
