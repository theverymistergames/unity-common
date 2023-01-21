using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintsEditorWindow : EditorWindow {

        private BlueprintsView _blueprintsView;
        private ObjectField _assetPicker;

        [MenuItem("MisterGames/Blueprints Editor")]
        private static void OpenWindow() {
            GetWindow();
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line) {
            if (Selection.activeObject is not BlueprintAsset blueprintAsset) return false;
            GetWindow().PopulateFromAsset(blueprintAsset);
            return true;
        }

        public static BlueprintsEditorWindow GetWindow() {
            return GetWindow<BlueprintsEditorWindow>("Blueprints Editor");
        }

        private void OnDisable() {
            _blueprintsView?.ClearView();
        }

        private void OnDestroy() {
            _blueprintsView?.ClearView();
        }

        public void PopulateFromAsset(BlueprintAsset blueprintAsset) {
            if (_blueprintsView == null) return;

            _assetPicker.value = blueprintAsset;
            _assetPicker.label = "Asset";
        }

        private void CreateGUI() {
            var root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("BlueprintsEditorView");
            visualTree.CloneTree(root); 

            var styleSheet = Resources.Load<StyleSheet>("BlueprintsEditorViewStyle");
            root.styleSheets.Add(styleSheet);

            _assetPicker = root.Q<ObjectField>("asset");
            _assetPicker.objectType = typeof(BlueprintAsset);
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

            var asset = evt.newValue;

            if (asset == null) {
                _blueprintsView.ClearView();
                return;
            }

            if (asset is BlueprintAsset blueprintAsset) {
                _blueprintsView.PopulateViewFromAsset(blueprintAsset);
            }
        }

        private Vector2 GetWorldPosition(Vector2 mousePosition) {
            var local = mousePosition - position.position;
            var world = rootVisualElement.LocalToWorld(local);
            return rootVisualElement.parent.WorldToLocal(world);
        }
    }
    
}
