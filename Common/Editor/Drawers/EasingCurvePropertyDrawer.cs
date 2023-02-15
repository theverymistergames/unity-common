using MisterGames.Common.Easing;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(EasingCurve))]
    public class EasingCurvePropertyDrawer : PropertyDrawer {

        private const float EASING_PROPERTY_X = 0.7f;
        private const float EASING_PROPERTY_PADDING = 8f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            float easingPropertyX = position.x + position.width * EASING_PROPERTY_X;
            float easingPropertyWidth = position.width * (1f - EASING_PROPERTY_X);

            var curveProperty = property.FindPropertyRelative("curve");
            var curveRect = new Rect(position.x, position.y, easingPropertyX - EASING_PROPERTY_PADDING, position.height);
            EditorGUI.PropertyField(curveRect, curveProperty, label);

            var easingProperty = property.FindPropertyRelative("_easingType");
            var easingRect = new Rect(easingPropertyX, position.y, easingPropertyWidth, position.height);
            EditorGUI.PropertyField(easingRect, easingProperty, GUIContent.none);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("curve"));
        }
    }

}
