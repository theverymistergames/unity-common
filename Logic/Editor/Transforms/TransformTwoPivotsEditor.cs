using MisterGames.Common.Colors;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Drawers;
using MisterGames.Logic.Transforms;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Logic.Editor.Transforms {

    [CustomEditor(typeof(TransformTwoPivots))]
    internal sealed class TransformTwoPivotsEditor : UnityEditor.Editor {

        private SerializedProperty _p0Property;
        private SerializedProperty _p1Property;
        private SerializedProperty _p0OffsetProperty;
        private SerializedProperty _p1OffsetProperty;
        private ButtonsDrawer _buttonsDrawer;

        private void OnEnable() {
            _buttonsDrawer = new ButtonsDrawer(target);
            _p0Property = serializedObject.FindProperty("_p0");
            _p1Property = serializedObject.FindProperty("_p1");
            _p0OffsetProperty = serializedObject.FindProperty("_p0Offset");
            _p1OffsetProperty = serializedObject.FindProperty("_p1Offset");
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            _buttonsDrawer.DrawButtons(targets);
        }

        private void OnSceneGUI() {
            serializedObject.Update();

            var p0 = DrawOffsetHandle(_p0Property, _p0OffsetProperty, "P0", new Color(1f, 0.85f, 0.2f).WithAlpha(0.3f));
            var p1 = DrawOffsetHandle(_p1Property, _p1OffsetProperty, "P1", new Color(0.2f, 0.85f, 1f).WithAlpha(0.3f));

            if (p0.HasValue && p1.HasValue) {
                var prevColor = Handles.color;
                Handles.color = Color.yellow;

                Handles.DrawDottedLine(p0.Value, p1.Value, 4f);

                Handles.color = prevColor;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static Optional<Vector3> DrawOffsetHandle(SerializedProperty pivotProperty, SerializedProperty offsetProperty, string label, Color color) {
            if (pivotProperty.objectReferenceValue is not Transform pivot) return Optional<Vector3>.Empty;

            var worldPos = pivot.position + pivot.rotation * offsetProperty.vector3Value;

            EditorGUI.BeginChangeCheck();
            var newWorldPos = Handles.PositionHandle(worldPos, pivot.rotation);

            if (EditorGUI.EndChangeCheck()) {
                offsetProperty.vector3Value = Quaternion.Inverse(pivot.rotation) * (newWorldPos - pivot.position);
                worldPos = newWorldPos;
            }

            var prevColor = Handles.color;
            Handles.color = color;

            Handles.SphereHandleCap(0, worldPos, Quaternion.identity, HandleUtility.GetHandleSize(worldPos) * 0.5f, EventType.Repaint);
            Handles.Label(worldPos, label);

            Handles.color = prevColor;

            return Optional<Vector3>.WithValue(worldPos);
        }
    }
    
}
