using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Easing;
using MisterGames.Common.Editor.Views;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Easing {

    [CustomPropertyDrawer(typeof(EasingType))]
    public class EasingTypePropertyDrawer : PropertyDrawer {

        private const float CURVE_PROPERTY_RATIO = 0.35f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var rect = position;
            rect.width = position.width * (1f - CURVE_PROPERTY_RATIO) - EditorGUIUtility.standardVerticalSpacing;
            
            var easingType = (EasingType) property.intValue;
            
            GUI.Label(rect, label);

            rect.x += EditorGUIUtility.labelWidth;
            rect.width -= EditorGUIUtility.labelWidth;
            
            if (EditorGUI.DropdownButton(rect, new GUIContent($"{easingType}"), FocusType.Keyboard)) {
                CreateEasingTypeDropdown(property).Show(rect);
            }
            
            rect = position;
            rect.x += position.width * (1f - CURVE_PROPERTY_RATIO) + EditorGUIUtility.standardVerticalSpacing;
            rect.width = position.width * CURVE_PROPERTY_RATIO;
            EditorGUI.CurveField(rect, easingType.ToAnimationCurve());
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property);
        }
        
        private static AdvancedDropdown<int> CreateEasingTypeDropdown(SerializedProperty property) {
            property = property.Copy();

            var easingTypes = new List<int>();
            easingTypes.AddRange(Enum.GetValues(typeof(EasingType)).Cast<int>());

            return new AdvancedDropdown<int>(
                $"Select {nameof(EasingType)}",
                easingTypes,
                i => $"{(EasingType) i}",
                (i, _) => {
                    property.intValue = i;

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                },
                separator: '.',
                sort: children => children.OrderBy(n => n.data.data)
            );
        }
    }

}
