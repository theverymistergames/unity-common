using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintsEditorWindow : EditorWindow {

        private const string WINDOW_TITLE = "BLueprint Editor";

        private BlueprintsView _blueprintsView;
        private ObjectField _assetPicker;

        public class SaveHelper : AssetModificationProcessor {
            public static string[] OnWillSaveAssets(string[] paths) {
                if (HasOpenInstances<BlueprintsEditorWindow>()) GetWindow().OnSaveCallback();
                return paths;
            }
        }

        [MenuItem("MisterGames/Blueprint Editor")]
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
            return GetWindow<BlueprintsEditorWindow>(WINDOW_TITLE);
        }

        private void OnDisable() {
            _blueprintsView?.ClearView();
            SetWindowTitle(WINDOW_TITLE);
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
            _blueprintsView.OnBlueprintAssetSetDirty = OnBlueprintAssetSetDirty;

            var blackboardToggle = root.Q<ToolbarToggle>("blackboard-toggle");
            var miniMapToggle = root.Q<ToolbarToggle>("minimap-toggle");

            blackboardToggle.RegisterValueChangedCallback(ToggleBlackboard);
            miniMapToggle.RegisterValueChangedCallback(ToggleMiniMap);
        }

        private void OnSaveCallback() {
            if (_assetPicker == null) return;

            var currentAsset = _assetPicker.value;
            if (currentAsset == null) return;

            SetWindowTitle(currentAsset.name);
        }

        private void OnBlueprintAssetSetDirty(BlueprintAsset blueprintAsset) {
            SetWindowTitle($"{blueprintAsset.name}*");
        }

        private void SetWindowTitle(string text) {
            titleContent = new GUIContent(text);
        }

        private void ToggleBlackboard(ChangeEvent<bool> evt) {
            _blueprintsView?.ToggleBlackboard(evt.newValue);
        }

        private void ToggleMiniMap(ChangeEvent<bool> evt) {
            _blueprintsView?.ToggleMiniMap(evt.newValue);
        }

        public override void SaveChanges() {
            Debug.Log($"BlueprintsEditorWindow.SaveChanges: ");

            base.SaveChanges();
        }

        public override void DiscardChanges() {
            Debug.Log($"BlueprintsEditorWindow.DiscardChanges: ");

            base.DiscardChanges();
        }

        private void OnAssetChanged(ChangeEvent<Object> evt) {
            if (_blueprintsView == null) {
                SetWindowTitle(WINDOW_TITLE);
                return;
            }

            var asset = evt.newValue;

            if (asset == null) {
                _blueprintsView.ClearView();
                SetWindowTitle(WINDOW_TITLE);
                return;
            }

            if (asset is not BlueprintAsset blueprintAsset) {
                SetWindowTitle(WINDOW_TITLE);
                return;
            }

            _blueprintsView.PopulateViewFromAsset(blueprintAsset);
            SetWindowTitle(blueprintAsset.name);
        }

        private Vector2 GetWorldPosition(Vector2 mousePosition) {
            var local = mousePosition - position.position;
            var world = rootVisualElement.LocalToWorld(local);
            return rootVisualElement.parent.WorldToLocal(world);
        }
    }
    
}
