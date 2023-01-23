using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintsEditorWindow : EditorWindow {

        private const string WINDOW_TITLE = "Blueprint Editor";

        private BlueprintsView _blueprintsView;
        private ObjectField _assetPicker;

        private class SaveHelper : AssetModificationProcessor {
            public static string[] OnWillSaveAssets(string[] paths) {
                if (!HasOpenInstances<BlueprintsEditorWindow>()) return paths;
                GetWindow<BlueprintsEditorWindow>(WINDOW_TITLE, focus: false).OnSaveCallback();
                return paths;
            }
        }

        [MenuItem("MisterGames/Blueprint Editor")]
        private static void OpenWindow() {
            GetWindow<BlueprintsEditorWindow>(WINDOW_TITLE, focus: true, desiredDockNextTo: typeof(SceneView));
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line) {
            if (Selection.activeObject is not BlueprintAsset blueprintAsset) return false;
            OpenAsset(blueprintAsset);
            return true;
        }

        public static void OpenAsset(BlueprintAsset blueprintAsset) {
            var window = GetWindow<BlueprintsEditorWindow>(WINDOW_TITLE, focus: true, desiredDockNextTo: typeof(SceneView));
            window.SetAssetPickerValue(blueprintAsset);
        }

        private void SetAssetPickerValue(BlueprintAsset blueprintAsset) {
            if (_assetPicker == null) return;

            _assetPicker.value = blueprintAsset;
            _assetPicker.label = "Asset";
        }

        private void OnEnable() {
            var asset = _assetPicker?.value as BlueprintAsset;
            if (asset == null) SetWindowTitle(WINDOW_TITLE);
        }

        private void OnDisable() {
            _blueprintsView?.ClearView();
        }

        private void OnDestroy() {
            _blueprintsView?.ClearView();
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
            _blueprintsView.OnBlueprintAssetSetDirty = OnBlueprintAssetSetDirty;

            var blackboardToggle = root.Q<ToolbarToggle>("blackboard-toggle");
            var miniMapToggle = root.Q<ToolbarToggle>("minimap-toggle");

            blackboardToggle.RegisterValueChangedCallback(OnToggleBlackboardButton);
            miniMapToggle.RegisterValueChangedCallback(OnToggleMiniMapButton);
        }

        private void OnSaveCallback() {
            var asset = _assetPicker?.value as BlueprintAsset;
            SetWindowTitle(asset == null ? WINDOW_TITLE : asset.name);
        }

        private void OnBlueprintAssetSetDirty() {
            var asset = _assetPicker?.value as BlueprintAsset;
            SetWindowTitle(asset == null ? WINDOW_TITLE : $"{asset.name}*");
        }

        private void OnToggleBlackboardButton(ChangeEvent<bool> evt) {
            _blueprintsView?.ToggleBlackboard(evt.newValue);
        }

        private void OnToggleMiniMapButton(ChangeEvent<bool> evt) {
            _blueprintsView?.ToggleMiniMap(evt.newValue);
        }

        private void OnAssetChanged(ChangeEvent<Object> evt) {
            if (_blueprintsView == null) {
                SetWindowTitle(WINDOW_TITLE);
                return;
            }

            var asset = evt.newValue as BlueprintAsset;
            if (asset == null) {
                SetWindowTitle(WINDOW_TITLE);
                _blueprintsView.ClearView();
                return;
            }

            SetWindowTitle($"{asset.name}{(EditorUtility.IsDirty(asset) ? "*" : string.Empty)}");
            _blueprintsView.PopulateViewFromAsset(asset);
        }

        private void SetWindowTitle(string text) {
            titleContent.text = text;
        }

        private Vector2 GetWorldPosition(Vector2 mousePosition) {
            var local = mousePosition - position.position;
            var world = rootVisualElement.LocalToWorld(local);
            return rootVisualElement.parent.WorldToLocal(world);
        }
    }
    
}
