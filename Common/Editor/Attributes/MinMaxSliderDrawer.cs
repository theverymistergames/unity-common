using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes {

    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public sealed class MinMaxSliderDrawer : PropertyDrawer {

        private const float ValueWidthRatio = 0.14f;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
           switch (property.propertyType) {
               case SerializedPropertyType.Vector2:
                   DrawSlider(position, property, label, asInteger: false);
                   return;
               
               case SerializedPropertyType.Vector2Int:
                   DrawSlider(position, property, label, asInteger: true);
                   return;
               
               default:
                   EditorGUI.LabelField(position, $"Use {nameof(MinMaxSliderAttribute)} with Vector2 or Vector2Int");
                   break;
           }
        }

        private void DrawSlider(Rect position, SerializedProperty property, GUIContent label, bool asInteger) {
            var attr = (MinMaxSliderAttribute) attribute;

            var vector2 = asInteger ? property.vector2IntValue : property.vector2Value;
            float x = Mathf.Clamp(vector2.x, attr.min, attr.max);
            float y = Mathf.Clamp(vector2.y, x, attr.max);

            GUI.Label(position, label);

            bool hasLabel = label != null && label != GUIContent.none;
            float offset = hasLabel.AsFloat() * (EditorGUIUtility.labelWidth + 2f);

            position.x += offset;
            position.width -= offset;

            var rect = position;
            rect.width = position.width * ValueWidthRatio - EditorGUIUtility.standardVerticalSpacing;
            x = asInteger ? EditorGUI.IntField(rect, (int) x) : EditorGUI.FloatField(rect, x);

            rect.x += position.width * (1f - ValueWidthRatio) + EditorGUIUtility.standardVerticalSpacing;
            y = asInteger ? EditorGUI.IntField(rect, (int) y) : EditorGUI.FloatField(rect, y);

            x = Mathf.Clamp(x, attr.min, attr.max);
            y = Mathf.Clamp(y, x, attr.max);

            rect = position;
            rect.x += position.width * ValueWidthRatio;
            rect.width = position.width * (1f - 2f * ValueWidthRatio);

            if (asInteger) {
                EditorGUI.MinMaxSlider(rect, GUIContent.none, ref x, ref y, (int) attr.min, (int) attr.max);
            }
            else {
                EditorGUI.MinMaxSlider(rect, GUIContent.none, ref x, ref y, attr.min, attr.max);
            }
            
            property.vector2Value = new Vector2(x, y);

            float sliderStart = position.x + position.width * ValueWidthRatio;
            float sliderEnd = position.x + position.width * (1f - ValueWidthRatio);

            string minText = asInteger ? $"{(int) attr.min}" : $"{attr.min.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}";
            string maxText = asInteger ? $"{(int) attr.max}" : $"{attr.max.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}";

            float minW = sliderEnd - sliderStart;
            float maxW = asInteger ? maxText.Length * 10f : 20f + maxText.Split('.')[0].Length * 10f;

            GUI.Label(new Rect(sliderStart + 10f, position.y, minW, EditorGUIUtility.singleLineHeight), minText);
            GUI.Label(new Rect(sliderEnd - maxW - 4f, position.y, maxW, EditorGUIUtility.singleLineHeight), maxText);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
        }
    }

}
