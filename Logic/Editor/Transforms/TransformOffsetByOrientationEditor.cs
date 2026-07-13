using MisterGames.Logic.Transforms;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Logic.Editor.Transforms {

    [CustomEditor(typeof(TransformOffsetByOrientation))]
    internal sealed class TransformOffsetByOrientationEditor : UnityEditor.Editor {

        private SerializedProperty _targetProperty;
        private SerializedProperty _pivotProperty;
        private SerializedProperty _offsetsProperty;
        private SerializedProperty _axisProperty;

        private void OnEnable() {
            _targetProperty = serializedObject.FindProperty("_target");
            _pivotProperty = serializedObject.FindProperty("_pivot");
            _offsetsProperty = serializedObject.FindProperty("_offsets");
            _axisProperty = serializedObject.FindProperty("_axis");
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
        }

        private void OnSceneGUI() {
            if (_pivotProperty.objectReferenceValue is not Transform pivot) return;
            if (_targetProperty.objectReferenceValue is not Transform target) return;

            serializedObject.Update();

            var pivotPos = pivot.position;
            var parentRot = target.parent != null ? target.parent.rotation : Quaternion.identity;
            int count = _offsetsProperty.arraySize;
            var axis = _axisProperty.vector3Value;
            
            for (int i = 0; i < count; i++) {
                var element = _offsetsProperty.GetArrayElementAtIndex(i);
                var orientationProperty = element.FindPropertyRelative("orientation");
                var offsetProperty = element.FindPropertyRelative("offset");

                var color = Color.HSVToRGB(i * 0.618034f % 1f, 0.7f, 1f);
                DrawOffsetHandle(pivotPos, parentRot, axis, orientationProperty, offsetProperty, $"Offset {i}", color);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawOffsetHandle(
            Vector3 pivotPos,
            Quaternion parentRot,
            Vector3 axis,
            SerializedProperty orientationProperty,
            SerializedProperty offsetProperty,
            string label,
            Color color) 
        {
            var localOrient = Quaternion.Euler(orientationProperty.vector3Value);
            var worldOrient = parentRot * localOrient;
            float offset = offsetProperty.floatValue;
            var worldPos = pivotPos + worldOrient * (axis * offset);

            EditorGUI.BeginChangeCheck();
            var newWorldPos = Handles.PositionHandle(worldPos, worldOrient);

            if (EditorGUI.EndChangeCheck()) {
                var toPivot = pivotPos - newWorldPos;
                float dist = toPivot.magnitude;

                if (dist > 0.0001f) {
                    var forward = toPivot / dist;
                    var upHint = worldOrient * Vector3.up;
                    if (Mathf.Abs(Vector3.Dot(forward, upHint)) > 0.999f) upHint = worldOrient * Vector3.right;

                    var newWorldOrient = Quaternion.LookRotation(forward, upHint);
                    var newLocalOrient = Quaternion.Inverse(parentRot) * newWorldOrient;

                    orientationProperty.vector3Value = newLocalOrient.eulerAngles;
                    offsetProperty.floatValue = -dist;
                }

                worldPos = newWorldPos;
            }

            var prevColor = Handles.color;
            Handles.color = color;

            Handles.DrawDottedLine(worldPos, pivotPos, 4f);
            Handles.SphereHandleCap(0, worldPos, Quaternion.identity, HandleUtility.GetHandleSize(worldPos) * 0.15f, EventType.Repaint);
            Handles.Label(worldPos, label);

            Handles.color = prevColor;
        }
    }

}
