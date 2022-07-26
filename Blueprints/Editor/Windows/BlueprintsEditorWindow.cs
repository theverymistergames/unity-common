using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Editor.Blueprints.Editor.Utils;
using MisterGames.Blueprints.Editor.Views;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MisterGames.Fsm.Editor.Windows {

    internal class BlueprintsEditorWindow : EditorWindow {

        private BlueprintsView _blueprintsView;

        private Label _graphHeader;

        private ObjectField _assetPicker;
        private Object _asset;
        
        [MenuItem("MisterGames/Blueprints Editor")]
        public static BlueprintsEditorWindow OpenWindow() {
            var window = GetWindow<BlueprintsEditorWindow>();
            window.titleContent = new GUIContent("Blueprints Editor");
            return window;
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line) {
            if (Selection.activeObject is Blueprint) {
                OpenWindow().OpenSelectedObject();
                return true;
            }
            return false;
        }

        public void PopulateFromAsset(Blueprint blueprint) {
            if (_blueprintsView == null) return;
            _assetPicker.value = blueprint;
            _assetPicker.label = "Asset";
        }

        public void PopulateFromHost(Object host) {
            if (_blueprintsView == null) return;
            _assetPicker.value = host;
            _assetPicker.label = "Asset (Runtime)";
        }

        private void OnEnable() {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private void OnDisable() {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        private void OnDestroy() {
            _blueprintsView?.OnDestroyEditorWindow();
        }

        private void CreateGUI() {
            var root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("BlueprintsEditorView");
            visualTree.CloneTree(root); 

            var styleSheet = Resources.Load<StyleSheet>("BlueprintsEditorViewStyle");
            root.styleSheets.Add(styleSheet);

            _assetPicker = root.Q<ObjectField>("asset");
            _assetPicker.objectType = typeof(Blueprint);
            _assetPicker.allowSceneObjects = false;
            _assetPicker.RegisterCallback<ChangeEvent<Object>>(OnAssetChanged);

            _blueprintsView = root.Q<BlueprintsView>();
            _blueprintsView.OnRequestWorldPosition = GetWorldPosition;
            var blackboardToggle = root.Q<ToolbarToggle>("blackboard-toggle");
            var miniMapToggle = root.Q<ToolbarToggle>("minimap-toggle");
            
            blackboardToggle.RegisterValueChangedCallback(ToggleBlackboard);
            miniMapToggle.RegisterValueChangedCallback(ToggleMiniMap);
        }

        private void ToggleBlackboard(ChangeEvent<bool> evt) {
            _blueprintsView?.ToggleBlackboard(evt.newValue);
        }

        private void ToggleMiniMap(ChangeEvent<bool> evt) {
            _blueprintsView?.ToggleMiniMap(evt.newValue);
        }

        private void OnAssetChanged(ChangeEvent<Object> evt) {
            if (_blueprintsView == null) return;

            var obj = evt.newValue;

            if (obj == null) {
                _blueprintsView.ClearView();
                return;
            }

            if (obj is Blueprint blueprint) {
                _blueprintsView.PopulateViewFromAsset(blueprint);
                return;
            }
            
            if (obj is IBlueprintHost host) {
                _blueprintsView.PopulateViewFromHost(host);
                return;
            }
        }

        private Vector2 GetWorldPosition(Vector2 mousePosition) {
            var local = mousePosition - position.position;
            var world = rootVisualElement.LocalToWorld(local);
            return rootVisualElement.parent.WorldToLocal(world);
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange mode) {
            switch (mode) {
                case PlayModeStateChange.EnteredEditMode:
                    _blueprintsView?.OnEnteredEditMode();
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
            }
        }

        private void OnSelectionChange() {
            if (_blueprintsView == null) return;
            if (_blueprintsView.IsAssetDestroyed()) ClearView();
        }

        private void OpenSelectedObject() {
            var blueprint = Selection.activeObject as Blueprint;
            if (blueprint != null) PopulateFromAsset(blueprint);
        }

        private void ClearView() {
            if (_blueprintsView == null) return;
            _assetPicker.value = null;
            _assetPicker.label = "Asset";
            _blueprintsView.ClearView();
        }
    }
    
}