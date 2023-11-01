using MisterGames.Blueprints.Editor.Storage;
using MisterGames.Blueprints.Editor.View;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MisterGames.Blueprints.Editor.Windows {

    public sealed class BlueprintEditorWindow : EditorWindow {

        private const string WINDOW_TITLE = "Blueprint Editor 2";

        private BlueprintsView _blueprintsView;
        private ObjectField _assetPicker;
        private Button _saveButton;

        private class SaveHelper : AssetModificationProcessor {
            public static string[] OnWillSaveAssets(string[] paths) {
                if (!HasOpenInstances<BlueprintEditorWindow>()) return paths;
                GetWindow<BlueprintEditorWindow>(WINDOW_TITLE, focus: false).TrySaveCurrentBlueprintAsset();
                return paths;
            }
        }

        [MenuItem("MisterGames/Blueprint Editor 2")]
        private static void OpenWindow() {
            GetWindow<BlueprintEditorWindow>(WINDOW_TITLE, focus: true, desiredDockNextTo: typeof(SceneView));
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line) {
            if (Selection.activeObject is not BlueprintAsset2 blueprintAsset) return false;

            OpenAsset(blueprintAsset);
            return true;
        }

        public static void OpenAsset(BlueprintAsset2 blueprintAsset) {
            var window = GetWindow<BlueprintEditorWindow>(WINDOW_TITLE, focus: true, desiredDockNextTo: typeof(SceneView));

            var assetPicker = window._assetPicker;
            if (assetPicker == null) return;

            assetPicker.value = blueprintAsset;
            assetPicker.label = "Asset";
        }

        private void OnDisable() {
            _blueprintsView?.ClearView();
            if (_saveButton != null) _saveButton.clicked -= TrySaveCurrentBlueprintAsset;
        }

        private void OnDestroy() {
            _blueprintsView?.ClearView();
            if (_saveButton != null) _saveButton.clicked -= TrySaveCurrentBlueprintAsset;
        }

        private void CreateGUI() {
            var root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("BlueprintsEditorView");
            visualTree.CloneTree(root); 

            var styleSheet = Resources.Load<StyleSheet>("BlueprintsEditorViewStyle");
            root.styleSheets.Add(styleSheet);

            _assetPicker = root.Q<ObjectField>("asset");
            _assetPicker.objectType = typeof(BlueprintAsset2);
            _assetPicker.allowSceneObjects = false;
            _assetPicker.RegisterCallback<ChangeEvent<Object>>(OnAssetChanged);

            _blueprintsView = root.Q<BlueprintsView>();
            _blueprintsView.OnRequestWorldPosition = GetWorldPosition;
            _blueprintsView.OnBlueprintAssetSetDirty = OnBlueprintAssetSetDirty;

            _saveButton = root.Q<Button>("save-button");
            _saveButton.clicked -= TrySaveCurrentBlueprintAsset;
            _saveButton.clicked += TrySaveCurrentBlueprintAsset;

            var blackboardToggle = root.Q<ToolbarToggle>("blackboard-toggle");
            blackboardToggle.RegisterValueChangedCallback(OnToggleBlackboardButton);

            OpenAsset(BlueprintEditorStorage.Instance.LastEditedBlueprintAsset);
        }

        private void OnBlueprintAssetSetDirty() {
            var asset = _assetPicker?.value as BlueprintAsset2;
            SetWindowTitle(asset == null ? WINDOW_TITLE : $"{asset.name}*");
        }

        private void TrySaveCurrentBlueprintAsset() {
            var asset = _assetPicker?.value as BlueprintAsset2;
            if (asset != null) AssetDatabase.SaveAssetIfDirty(asset);
            SetWindowTitle(asset == null ? WINDOW_TITLE : asset.name);
        }

        private void OnToggleBlackboardButton(ChangeEvent<bool> evt) {
            _blueprintsView?.ToggleBlackboard(evt.newValue);
        }

        private void OnAssetChanged(ChangeEvent<Object> evt) {
            if (_blueprintsView == null) {
                BlueprintEditorStorage.Instance.NotifyOpenedBlueprintAsset(null);
                SetWindowTitle(WINDOW_TITLE);
                return;
            }

            var asset = evt.newValue as BlueprintAsset2;
            if (asset == null) {
                BlueprintEditorStorage.Instance.NotifyOpenedBlueprintAsset(null);
                SetWindowTitle(WINDOW_TITLE);
                _blueprintsView.ClearView();
                return;
            }

            BlueprintEditorStorage.Instance.NotifyOpenedBlueprintAsset(asset);
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
