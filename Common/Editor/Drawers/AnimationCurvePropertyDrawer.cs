using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Easing;
using MisterGames.Common.Editor.Views;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(AnimationCurve))]
    public class AnimationCurvePropertyDrawer : PropertyDrawer {

        private const float EASING_PROPERTY_X = 0.7f;
        private const float EASING_PROPERTY_PADDING = 8f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            float easingPropertyX = position.x + position.width * EASING_PROPERTY_X;
            float easingPropertyWidth = position.width * (1f - EASING_PROPERTY_X);

            var curveRect = new Rect(position.x, position.y, easingPropertyX - EASING_PROPERTY_PADDING, position.height);
            EditorGUI.PropertyField(curveRect, property, label);

            var easingLabel = property.animationCurveValue.TryGetEasingType(out var easingType)
                ? new GUIContent($"{easingType}")
                : new GUIContent("Custom");

            var easingRect = new Rect(easingPropertyX, position.y, easingPropertyWidth, position.height);
            if (EditorGUI.DropdownButton(easingRect, easingLabel, FocusType.Keyboard)) {
                CreateEasingTypeDropdown(property).Show(easingRect);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property);
        }

        private static AdvancedDropdown<int> CreateEasingTypeDropdown(SerializedProperty property) {
            property = property.Copy();

            var easingTypes = new List<int> { -1 };
            easingTypes.AddRange(Enum.GetValues(typeof(EasingType)).Cast<int>());

            return new AdvancedDropdown<int>(
                $"Select {nameof(EasingType)}",
                easingTypes,
                i => i < 0 ? "Custom" : $"{(EasingType) i}",
                i => {
                    if (i < 0) return;

                    property.animationCurveValue = ((EasingType) i).ToAnimationCurve();

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                },
                separator: '.',
                sort: children => children.OrderBy(n => n.data.data)
            );
        }
    }

}
