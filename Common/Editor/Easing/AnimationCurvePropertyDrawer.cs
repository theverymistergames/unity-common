using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Easing;
using MisterGames.Common.Editor.Views;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Easing {

    [CustomPropertyDrawer(typeof(AnimationCurve))]
    public class AnimationCurvePropertyDrawer : PropertyDrawer {

        private const float EASING_PROPERTY_RATIO = 0.25f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var rect = position;
            rect.width = position.width * (1f - EASING_PROPERTY_RATIO) - EditorGUIUtility.standardVerticalSpacing;
            
            EditorGUI.PropertyField(rect, property, label);

            var easingLabel = property.animationCurveValue.TryGetEasingType(out var easingType)
                ? new GUIContent($"{easingType}")
                : new GUIContent("Custom");

            rect = position;
            rect.x += position.width * (1f - EASING_PROPERTY_RATIO) + EditorGUIUtility.standardVerticalSpacing;
            rect.width = position.width * EASING_PROPERTY_RATIO;

            if (EditorGUI.DropdownButton(rect, easingLabel, FocusType.Keyboard)) {
                CreateEasingTypeDropdown(property).Show(rect);
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
                (i, _) => {
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
    
    [InitializeOnLoad]
    internal static class AnimationCurveContextMenu {
        
        static AnimationCurveContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.AnimationCurve) return;

            var curve = property.animationCurveValue;
            
            menu.AddItem(new GUIContent("Invert curve by X"), false, () => {
                curve.InvertAnimationCurveX();
                
                property.animationCurveValue = curve;
                
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                        
                EditorUtility.SetDirty(property.serializedObject.targetObject); 
            });
            
            menu.AddItem(new GUIContent("Invert curve by Y"), false, () => {
                curve.InvertAnimationCurveY();
                
                property.animationCurveValue = curve;
                
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                        
                EditorUtility.SetDirty(property.serializedObject.targetObject); 
            });
        }
    }

}
