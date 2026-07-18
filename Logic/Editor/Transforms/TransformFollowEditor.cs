using MisterGames.Common.Colors;
using MisterGames.Common.Editor.Drawers;
using MisterGames.Logic.Transforms;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Logic.Editor.Transforms {

    [CustomEditor(typeof(TransformFollow))]
    internal sealed class TransformFollowEditor : UnityEditor.Editor {

        private SerializedProperty _followProperty;
        private SerializedProperty _positionOffsetProperty;
        private SerializedProperty _rotationOffsetProperty;
        private SerializedProperty _showGizmoProperty;
        private ButtonsDrawer _buttonsDrawer;

        private void OnEnable() {
            _buttonsDrawer = new ButtonsDrawer(target);
            _followProperty = serializedObject.FindProperty("_follow");
            _positionOffsetProperty = serializedObject.FindProperty("_positonOffset");
            _rotationOffsetProperty = serializedObject.FindProperty("_rotationOffset");
            _showGizmoProperty = serializedObject.FindProperty("_showGizmo");
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            _buttonsDrawer.DrawButtons(targets);
        }

        private const float HANDLE_OFFSET_SCALE = 2f;

        private void OnSceneGUI() {
            if (_followProperty.objectReferenceValue is not Transform follow ||
                !_showGizmoProperty.boolValue) {
                return;
            }

            serializedObject.Update();

            var followRot = follow.rotation;
            var worldRot = followRot * Quaternion.Euler(_rotationOffsetProperty.vector3Value);
            var worldPos = follow.position + followRot * _positionOffsetProperty.vector3Value;

            var handleOffset = followRot * Vector3.up * (HandleUtility.GetHandleSize(worldPos) * HANDLE_OFFSET_SCALE);
            var handlePos = worldPos + handleOffset;

            EditorGUI.BeginChangeCheck();
            var newHandlePos = Handles.PositionHandle(handlePos, worldRot);
            if (EditorGUI.EndChangeCheck()) {
                var newWorldPos = newHandlePos - handleOffset;
                _positionOffsetProperty.vector3Value = Quaternion.Inverse(followRot) * (newWorldPos - follow.position);
                handlePos = newHandlePos;
            }

            EditorGUI.BeginChangeCheck();
            var newWorldRot = Handles.RotationHandle(worldRot, handlePos);
            if (EditorGUI.EndChangeCheck()) {
                _rotationOffsetProperty.vector3Value = (Quaternion.Inverse(followRot) * newWorldRot).eulerAngles;
            }
            
            var prevColor = Handles.color;
            Handles.color = Color.yellow.WithAlpha(0.3f);

            Handles.DrawDottedLine(handlePos, worldPos, 4f);
            Handles.SphereHandleCap(0, handlePos, Quaternion.identity, HandleUtility.GetHandleSize(handlePos) * 0.5f, EventType.Repaint);
            Handles.SphereHandleCap(0, worldPos, Quaternion.identity, HandleUtility.GetHandleSize(handlePos) * 0.1f, EventType.Repaint);

            Handles.color = prevColor;
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
