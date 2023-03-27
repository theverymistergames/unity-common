using System;
using System.Reflection;
using MisterGames.Blackboards.Core;
using MisterGames.Common.Editor.Drawers;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Editor {

    [CustomPropertyDrawer(typeof(BlackboardValue<>))]
    public sealed class BlackboardValuePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            if (BlackboardUtils.TryGetBlackboardProperty(property, out var blackboardProperty, out _, out _)) {
                var type = (Type) blackboardProperty.type;
                if (type.IsArray) type = type.GetElementType();

                TypedField(position, property.FindPropertyRelative("value"), type, label);
            }
            else {
                BlackboardUtils.NullPropertyField(position, label);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return BlackboardUtils.TryGetBlackboardProperty(property, out var blackboardProperty, out _, out _)
                ? GetTypedFieldHeight(property, label, blackboardProperty.type)
                : BlackboardUtils.GetNullPropertyHeight();
        }

        private static void TypedField(Rect position, SerializedProperty property, Type type, GUIContent label) {
            if (typeof(Object).IsAssignableFrom(type)) {
                EditorGUI.ObjectField(position, property, type, label);
                return;
            }

            if (type.IsEnum) {
                var currentEnumValue = Enum.ToObject(type, property.longValue) as Enum;

                var result = type.GetCustomAttribute<FlagsAttribute>(false) != null
                    ? EditorGUI.EnumFlagsField(position, label, currentEnumValue)
                    : EditorGUI.EnumPopup(position, label, currentEnumValue);

                if (!Equals(result, currentEnumValue)) {
                    property.longValue = Convert.ToInt64(result);

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }

                return;
            }

            PropertyDrawerUtils.DrawPropertyField(
                position,
                property,
                label,
                SerializedPropertyExtensions.GetPropertyFieldInfo(property),
                includeChildren: true
            );
        }

        private static float GetTypedFieldHeight(SerializedProperty property, GUIContent label, Type type) {
            if (typeof(Object).IsAssignableFrom(type)) {
                return EditorGUI.GetPropertyHeight(property);
            }

            if (type.IsEnum) {
                return EditorGUIUtility.singleLineHeight;
            }

            return PropertyDrawerUtils.GetPropertyHeight(
                property,
                label,
                SerializedPropertyExtensions.GetPropertyFieldInfo(property),
                includeChildren: true
            );
        }
    }

}
