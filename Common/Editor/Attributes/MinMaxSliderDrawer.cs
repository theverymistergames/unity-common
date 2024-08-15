using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes {

    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var attr = (MinMaxSliderAttribute) attribute;
            
            if (property.propertyType is SerializedPropertyType.Vector2) {
                var vector2 = property.vector2Value;
                float x = vector2.x;
                float y = vector2.y;

                label = attr.show ? new GUIContent($"{label.text} [{attr.min}, {attr.max}]") : label;
                
                EditorGUI.MinMaxSlider(position, label, ref x, ref y, attr.min, attr.max);
                property.vector2Value = new Vector2(x, y);

                float sliderStart = position.x + EditorGUIUtility.labelWidth;
                float sliderEnd = position.x + position.width;

                string minText = $"{x.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}";
                string maxText = $"{y.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}";

                float minW = sliderEnd - sliderStart;
                float maxW = 20f + maxText.Split('.')[0].Length * 10f;
                
                GUI.Label(new Rect(sliderStart + 10f, position.y, minW, EditorGUIUtility.singleLineHeight), minText);
                GUI.Label(new Rect(sliderEnd - maxW - 4f, position.y, maxW, EditorGUIUtility.singleLineHeight), maxText);
                
                return;
            }

            if (property.propertyType is SerializedPropertyType.Vector2Int) {
                var vector2 = property.vector2IntValue;
                float x = vector2.x;
                float y = vector2.y;
                
                label = attr.show ? new GUIContent($"{label.text} [{(int) attr.min}, {(int) attr.max}]") : label;
                
                EditorGUI.MinMaxSlider(position, label, ref x, ref y, (int) attr.min, (int) attr.max);
                property.vector2IntValue = new Vector2Int((int) x, (int) y);
                
                float sliderStart = position.x + EditorGUIUtility.labelWidth;
                float sliderEnd = position.x + position.width;

                string minText = $"{(int) x}";
                string maxText = $"{(int) y}";

                float minW = sliderEnd - sliderStart;
                float maxW = maxText.Length * 10f;
                
                GUI.Label(new Rect(sliderStart + 10f, position.y, minW, EditorGUIUtility.singleLineHeight), minText);
                GUI.Label(new Rect(sliderEnd - maxW - 4f, position.y, maxW, EditorGUIUtility.singleLineHeight), maxText);
                
                return;
            }
            
            EditorGUI.LabelField(position, $"Use {nameof(MinMaxSliderAttribute)} with Vector2 or Vector2Int");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
        }
    }

}
