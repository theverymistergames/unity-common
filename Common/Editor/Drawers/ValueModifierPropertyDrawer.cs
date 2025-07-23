using MisterGames.Common.Stats;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(ValueModifier))]
    public class ValueModifierPropertyDrawer : PropertyDrawer {

        private const float WidthRatioA = 0.8f;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(
                position.x,
                position.y,
                EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight
            );
            
            var valueRect = new Rect(
                position.x + EditorGUIUtility.labelWidth,
                position.y,
                position.width - EditorGUIUtility.labelWidth,
                position.height
            );
            
            float widthA = valueRect.width * WidthRatioA;
            float widthB = valueRect.width * (1f - WidthRatioA);
            
            var modifierProperty = property.FindPropertyRelative(nameof(ValueModifier.modifier));
            var modifierRect = new Rect(labelRect.x, labelRect.y, labelRect.width + widthA - EditorGUIUtility.standardVerticalSpacing, valueRect.height);

            modifierProperty.floatValue = EditorGUI.FloatField(modifierRect, label, modifierProperty.floatValue);
            
            var operationRect = new Rect(valueRect.x + widthA, valueRect.y, widthB, valueRect.height);
            var operationProperty = property.FindPropertyRelative(nameof(ValueModifier.operation));

            EditorGUI.PropertyField(operationRect, operationProperty, GUIContent.none, includeChildren: true);

            EditorGUI.EndProperty();
        }
 
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
