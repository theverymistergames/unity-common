using MisterGames.Common.Easing;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(EasingType))]
    public class EasingTypePropertyDrawer : PropertyDrawer {

        private const float CURVE_PROPERTY_RATIO = 0.35f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var rect = position;
            rect.width = position.width * (1f - CURVE_PROPERTY_RATIO) - EditorGUIUtility.standardVerticalSpacing;
            
            EditorGUI.PropertyField(rect, property, label);
            
            rect = position;
            rect.x += position.width * (1f - CURVE_PROPERTY_RATIO) + EditorGUIUtility.standardVerticalSpacing;
            rect.width = position.width * CURVE_PROPERTY_RATIO;
            
            var easingType = (EasingType) property.intValue;
            EditorGUI.CurveField(rect, easingType.ToAnimationCurve());
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property);
        }
    }

}
