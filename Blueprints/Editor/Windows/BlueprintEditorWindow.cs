using System.Linq;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Editor.Storage;
using MisterGames.Blueprints.Editor.View;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Meta;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MisterGames.Blueprints.Editor.Windows {

    public sealed class BlueprintEditorWindow : EditorWindow {

        private const string WINDOW_TITLE = "Blueprint Editor 2";

        private BlueprintMeta2 _blueprintMeta;
        private IBlueprintFactory _factoryOverride;
        private Blackboard _blackboard;
        private SerializedObject _serializedObject;

        private BlueprintsView _blueprintsView;
        private ObjectField _assetPicker;
        private ObjectField _hostPicker;
        private Label _hostPickerLabel;
        private Label _address;

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

            Open(blueprintAsset);
            return true;
        }

        public static void Open(
            BlueprintAsset2 asset = null,
            BlueprintMeta2 meta = null,
            IBlueprintFactory factoryOverride = null,
            Blackboard blackboard = null,
            SerializedObject serializedObject = null
        ) {
            var window = GetWindow<BlueprintEditorWindow>(WINDOW_TITLE, focus: true, desiredDockNextTo: typeof(SceneView));

            if (asset != null) {
                meta ??= asset.BlueprintMeta;
                blackboard ??= asset.Blackboard;
                serializedObject ??= new SerializedObject(asset);
            }

            window._blueprintMeta = meta;
            window._blackboard = blackboard;
            window._factoryOverride = factoryOverride;
            window._serializedObject = serializedObject;

            window.SetAssetPickerValue(asset, notify: false);
            window.SetupView();
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
            _assetPicker.objectType = typeof(BlueprintAsset2);
            _assetPicker.allowSceneObjects = false;
            _assetPicker.RegisterCallback<ChangeEvent<Object>>(OnAssetChanged);

            _hostPickerLabel = root.Q<Label>("host-label");
            _hostPickerLabel.visible = false;

            _hostPicker = root.Q<ObjectField>("host");
            _hostPicker.objectType = typeof(Object);
            _hostPicker.allowSceneObjects = true;
            _hostPicker.SetEnabled(false);
            _hostPicker.visible = false;

            _address = root.Q<Label>("address");
            _address.visible = false;
            _address.text = null;

            _blueprintsView = root.Q<BlueprintsView>();
            _blueprintsView.OnRequestWorldPosition = GetWorldPosition;
            _blueprintsView.OnSetDirty = OnTargetObjectSetDirty;

            var blackboardToggle = root.Q<ToolbarToggle>("blackboard-toggle");
            blackboardToggle.RegisterValueChangedCallback(OnToggleBlackboardButton);

            BlueprintEditorStorage.Instance.OpenLast();
        }

        private void OnTargetObjectSetDirty(Object obj) {
            SetWindowTitle(_assetPicker?.value is BlueprintAsset2 asset ? $"{asset.name}*" : WINDOW_TITLE);
        }

        private void TrySaveCurrentBlueprintAsset() {
            var asset = _assetPicker?.value as BlueprintAsset2;
            if (asset != null) AssetDatabase.SaveAssetIfDirty(asset);
            SetWindowTitle(asset == null ? WINDOW_TITLE : asset.name);
        }

        private void OnToggleBlackboardButton(ChangeEvent<bool> evt) {
            _blueprintsView?.ToggleBlackboard(evt.newValue);
        }

        private void SetAssetPickerValue(BlueprintAsset2 asset, bool notify) {
            if (_assetPicker == null) return;

            if (notify) _assetPicker.value = asset;
            else _assetPicker.SetValueWithoutNotify(asset);
        }

        private void OnAssetChanged(ChangeEvent<Object> evt) {
            var asset = evt.newValue as BlueprintAsset2;

            if (asset == null) {
                _serializedObject = null;
                _blueprintMeta = null;
                _blackboard = null;
            }
            else {
                _serializedObject = new SerializedObject(asset);
                _blueprintMeta = asset.BlueprintMeta;
                _blackboard = asset.Blackboard;
            }

            _factoryOverride = null;

            SetupView();
        }

        private void SetupView() {
            if (_blueprintsView == null || _blueprintMeta == null) {
                _hostPickerLabel.visible = false;
                _hostPicker.visible = false;
                _hostPicker.value = null;
                _address.visible = false;
                _address.text = null;

                BlueprintEditorStorage.Instance.NotifyOpenedBlueprintAsset(null);
                SetWindowTitle(WINDOW_TITLE);
                _blueprintsView?.ClearView();
                return;
            }

            var asset = _assetPicker.value as BlueprintAsset2;
            string title = asset != null ? EditorUtility.IsDirty(asset) ? $"{asset.name}*" : asset.name : WINDOW_TITLE;
            SetWindowTitle(title);

            if (_serializedObject?.targetObject is BlueprintRunner2 runner) {
                var subgraphPath = runner.FindSubgraphPath(_blueprintMeta);
                BlueprintEditorStorage.Instance.NotifyOpenedBlueprintAsset(asset, runner, subgraphPath);

                string path = subgraphPath is { Length: > 0 }
                    ? $"Node {string.Join(" => ", subgraphPath.Select(id => $"{id.source}.{id.node}"))}"
                    : null;

                _hostPickerLabel.visible = true;
                _hostPicker.visible = true;
                _hostPicker.value = runner;
                _address.visible = true;
                _address.text = path;
            }
            else {
                BlueprintEditorStorage.Instance.NotifyOpenedBlueprintAsset(asset);

                _hostPickerLabel.visible = false;
                _hostPicker.visible = false;
                _hostPicker.value = null;
                _address.visible = false;
                _address.text = null;
            }

            _blueprintsView.PopulateView(_blueprintMeta, _factoryOverride, _blackboard, _serializedObject);
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
