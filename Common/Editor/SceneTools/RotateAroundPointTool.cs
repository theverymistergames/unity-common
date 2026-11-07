using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.SceneTools {

    [InitializeOnLoad]
    internal static class RotateAroundPointTool {

        private const float DoublePressTime = 0.3f;
        private const float PivotSizeFactor = 0.1f;
        private const string GlobalEventHandlerField = "globalEventHandler";
        private const string UndoName = "Rotate Around Point";

        private const EventModifiers BlockingModifiers =
            EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift | EventModifiers.Command;

        private static readonly Color PivotColor = new(1f, 0.6f, 0f, 1f);

        private static bool _isActive;
        private static bool _isActivationPending;
        private static bool _isToolsHidden;
        private static bool _isKeyHeld;
        private static double _lastPressTime;
        private static Vector3 _pivot;
        private static Quaternion _handleRotation;
        private static Vector2 _mousePosition;
        private static Transform _target;

        static RotateAroundPointTool() {
            SubscribeGlobalEventHandler();

            SceneView.duringSceneGui -= OnSceneGui;
            SceneView.duringSceneGui += OnSceneGui;

            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;

            AssemblyReloadEvents.beforeAssemblyReload -= Deactivate;
            AssemblyReloadEvents.beforeAssemblyReload += Deactivate;
        }

        private static void SubscribeGlobalEventHandler() {
            var field = typeof(EditorApplication).GetField(GlobalEventHandlerField, BindingFlags.Static | BindingFlags.NonPublic);

            if (field == null) {
                Debug.LogWarning($"{nameof(RotateAroundPointTool)}: field {nameof(EditorApplication)}.{GlobalEventHandlerField} is not found, double press E will not work.");
                return;
            }

            var handler = field.GetValue(null) as EditorApplication.CallbackFunction;
            handler -= OnGlobalEvent;
            field.SetValue(null, (EditorApplication.CallbackFunction) OnGlobalEvent + handler);
        }

        private static void OnGlobalEvent() {
            var e = Event.current;
            if (e == null || e.keyCode != KeyCode.E) return;

            if (e.type == EventType.KeyUp) {
                _isKeyHeld = false;
                return;
            }

            if (e.type != EventType.KeyDown ||
                _isKeyHeld ||
                (e.modifiers & BlockingModifiers) != 0 ||
                EditorGUIUtility.editingTextField ||
                EditorWindow.focusedWindow is not SceneView)
            {
                return;
            }

            _isKeyHeld = true;

            if (_isActive || _isActivationPending) {
                Deactivate();
                _lastPressTime = 0d;
                return;
            }

            double time = EditorApplication.timeSinceStartup;
            bool isDoublePress = time - _lastPressTime <= DoublePressTime;
            _lastPressTime = time;

            if (!isDoublePress || Selection.activeTransform == null) return;

            _lastPressTime = 0d;
            _isActivationPending = true;

            SceneView.RepaintAll();
        }

        private static void OnSelectionChanged() {
            if ((_isActive || _isActivationPending) && Selection.activeTransform != _target) Deactivate();
        }

        private static void OnSceneGui(SceneView sceneView) {
            var e = Event.current;
            if (e.isMouse) _mousePosition = e.mousePosition;

            if (_isActivationPending && EditorWindow.focusedWindow == sceneView) Activate(sceneView);
            if (!_isActive) return;

            if (Tools.current != Tool.Rotate || _target == null || Selection.activeTransform != _target) {
                Deactivate();
                return;
            }

            DrawPivotHandle(e);
            DrawRotationHandle();
        }

        private static void Activate(SceneView sceneView) {
            _isActivationPending = false;
            _target = Selection.activeTransform;

            if (_target == null || Tools.current != Tool.Rotate) return;

            _pivot = HandleUtility.PlaceObject(_mousePosition, out var point, out _) ? point : Tools.handlePosition;
            _isActive = true;
            _isToolsHidden = true;
            Tools.hidden = true;

            sceneView.Repaint();
        }

        private static void Deactivate() {
            _isActive = false;
            _isActivationPending = false;
            _target = null;

            if (_isToolsHidden) {
                _isToolsHidden = false;
                Tools.hidden = false;
            }

            SceneView.RepaintAll();
        }

        private static void DrawPivotHandle(Event e) {
            if (e.type == EventType.Repaint) {
                float size = HandleUtility.GetHandleSize(_pivot) * PivotSizeFactor;

                using (new Handles.DrawingScope(PivotColor)) {
                    Handles.SphereHandleCap(0, _pivot, Quaternion.identity, size, EventType.Repaint);
                }
            }

            var handleRotation = Tools.pivotRotation == PivotRotation.Local ? _target.rotation : Quaternion.identity;

            EditorGUI.BeginChangeCheck();
            var position = Handles.PositionHandle(_pivot, handleRotation);
            if (EditorGUI.EndChangeCheck()) _pivot = position;
        }

        private static void DrawRotationHandle() {
            if (GUIUtility.hotControl == 0) {
                _handleRotation = Tools.pivotRotation == PivotRotation.Local ? _target.rotation : Quaternion.identity;
            }

            EditorGUI.BeginChangeCheck();
            var rotation = Handles.RotationHandle(_handleRotation, _pivot);
            if (!EditorGUI.EndChangeCheck()) return;

            var delta = rotation * Quaternion.Inverse(_handleRotation);
            _handleRotation = rotation;

            delta.ToAngleAxis(out float angle, out var axis);
            if (float.IsInfinity(axis.x) || float.IsNaN(axis.x)) return;

            var transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
            Undo.RecordObjects(transforms, UndoName);

            for (int i = 0; i < transforms.Length; i++) {
                transforms[i].RotateAround(_pivot, axis, angle);
            }
        }
    }

}
