using MisterGames.Common.Colors;
using MisterGames.Common.Easing;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Common.Maths;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(OscillatedCurve))]
    public sealed class OscillatedCurvePropertyDrawer : PropertyDrawer {

        private const int GraphResolution = 200;
        private const float GraphHeight = 120f;

        private static readonly EditorGraph _graph = new(0f, 1f, 0f, 1f, GraphResolution);
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label, includeChildren: true);
            EditorGUI.EndProperty();

            if (!property.isExpanded) return;

            var curve = new OscillatedCurve {
                curve0 = property.FindPropertyRelative(nameof(OscillatedCurve.curve0)).animationCurveValue,
                curve1 = property.FindPropertyRelative(nameof(OscillatedCurve.curve1)).animationCurveValue,
                scale0 = property.FindPropertyRelative(nameof(OscillatedCurve.scale0)).floatValue,
                scale1 = property.FindPropertyRelative(nameof(OscillatedCurve.scale1)).floatValue,
                oscillateFreq0 = property.FindPropertyRelative(nameof(OscillatedCurve.oscillateFreq0)).floatValue,
                oscillateFreq1 = property.FindPropertyRelative(nameof(OscillatedCurve.oscillateFreq1)).floatValue,
                oscillateThr = property.FindPropertyRelative(nameof(OscillatedCurve.oscillateThr)).floatValue,
                phase = property.FindPropertyRelative(nameof(OscillatedCurve.phase)).floatValue, 
            };

            float max = float.MinValue;
            float min = float.MaxValue;
            
            for (float t = 0; t <= 1f; t += 1f / GraphResolution) {
                float v = curve.Evaluate(t);
                if (v > max) max = v;
                if (v < min) min = v;
            }
            
            float range = Mathf.Max(1f, Mathf.Max(Mathf.Abs(max), Mathf.Abs(min)));
            
            _graph.Clear();
            _graph.SetRangeX(0f, 1f);
            _graph.SetRangeY(-range, range);
            
            _graph.AddFunction(t => curve.Evaluate(t), Color.green);
            _graph.AddLineY(curve.oscillateThr, Color.red.WithAlpha(0.5f));
            _graph.AddLineY(-curve.oscillateThr, Color.red.WithAlpha(0.5f));
            _graph.AddLineY(0f, Color.gray.WithAlpha(0.3f));
            _graph.AddLineY(1f, Color.gray.WithAlpha(0.5f));
            _graph.AddLineY(-1f, Color.gray.WithAlpha(0.5f));

            EditorGUI.indentLevel++;
            
            position.x += EditorGUI.indentLevel * 15f;
            position.width -= EditorGUI.indentLevel * 15f;
            position.y += position.height - GraphHeight;
            position.height = GraphHeight;
            
            EditorGUI.indentLevel--;
            
            _graph.Draw(position);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property) + 
                   property.isExpanded.AsFloat() * (EditorGUIUtility.standardVerticalSpacing * 2f + GraphHeight);
        }
    }

}
