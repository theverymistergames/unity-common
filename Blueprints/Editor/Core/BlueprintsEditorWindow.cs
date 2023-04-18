using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintsEditorWindow : EditorWindow {

        private const string WINDOW_TITLE = "Blueprint Editor";

        private BlueprintsView _blueprintsView;
        private ObjectField _assetPicker;
        private ToolbarToggle _saveToggle;

        private class SaveHelper : AssetModificationProcessor {
            public static string[] OnWillSaveAssets(string[] paths) {
                if (!HasOpenInstances<BlueprintsEditorWindow>()) return paths;
                GetWindow<BlueprintsEditorWindow>(WINDOW_TITLE, focus: false).OnSaveCalled();
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

            if (asset == null) {
                _blueprintsView?.ClearView();
                SetWindowTitle(WINDOW_TITLE);
                return;
            }

            SetWindowTitle(EditorUtility.IsDirty(asset) ? $"{asset.name}*" : asset.name);
        }

        private void OnDisable() {
            _blueprintsView?.ClearView();

            rootVisualElement.Q<Button>("save-button").clicked -= OnClickSaveButton;
        }

        private void OnDestroy() {
            _blueprintsView?.ClearView();

            rootVisualElement.Q<Button>("save-button").clicked -= OnClickSaveButton;
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

            var saveButton = root.Q<Button>("save-button");
            saveButton.clicked -= OnClickSaveButton;
            saveButton.clicked += OnClickSaveButton;

            var blackboardToggle = root.Q<ToolbarToggle>("blackboard-toggle");
            blackboardToggle.RegisterValueChangedCallback(OnToggleBlackboardButton);
        }

        private void OnBlueprintAssetSetDirty() {
            var asset = _assetPicker?.value as BlueprintAsset;
            SetWindowTitle(asset == null ? WINDOW_TITLE : $"{asset.name}*");
        }

        private void OnSaveCalled() {
            var asset = _assetPicker?.value as BlueprintAsset;
            SetWindowTitle(asset == null ? WINDOW_TITLE : asset.name);
        }

        private void OnClickSaveButton() {
            AssetDatabase.SaveAssets();
            OnSaveCalled();
        }

        private void OnToggleBlackboardButton(ChangeEvent<bool> evt) {
            _blueprintsView?.ToggleBlackboard(evt.newValue);
        }

        private void OnSelectionChange() {
            if (_assetPicker == null) return;

            var asset = _assetPicker.value as BlueprintAsset;
            if (asset != null) return;

            _assetPicker.value = null;
            _blueprintsView?.ClearView();
            SetWindowTitle(WINDOW_TITLE);
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

            SetWindowTitle(EditorUtility.IsDirty(asset) ? $"{asset.name}*" : asset.name);
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
