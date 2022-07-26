using MisterGames.Common.Editor.Views;
using MisterGames.Fsm;
using MisterGames.Fsm.Core;
using MisterGames.Fsm.Editor.Views;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MisterGames.Fsm.Editor.Windows {

    internal class FsmEditorWindow : EditorWindow {

        private FsmView _fsmView;
        private InspectorView _inspectorView;

        private Label _graphHeader;
        private Label _inspectorObject;

        private ObjectField _assetPicker;
        private Object _asset;
        
        [MenuItem("MisterGames/State Machine Editor")]
        private static FsmEditorWindow OpenWindow() {
            var window = GetWindow<FsmEditorWindow>();
            window.titleContent = new GUIContent("State Machine Editor");
            return window;
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line) {
            if (Selection.activeObject is StateMachine) {
                OpenWindow().OpenSelectedObject();
                return true;
            }
            return false;
        }

        private void OnEnable() {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private void OnDisable() {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        private void OnDestroy() {
            _fsmView?.OnDestroyEditorWindow();
        }

        private void CreateGUI() {
            var root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("FsmEditorView");
            visualTree.CloneTree(root);

            var styleSheet = Resources.Load<StyleSheet>("FsmEditorViewStyle");
            root.styleSheets.Add(styleSheet);

            _fsmView = root.Q<FsmView>();
            _inspectorView = root.Q<InspectorView>();
            _inspectorObject = root.Q<Label>("object-inspector");
            
            _assetPicker = root.Q<ObjectField>("asset");
            _assetPicker.objectType = typeof(StateMachine);
            _assetPicker.allowSceneObjects = false;
            _assetPicker.RegisterCallback<ChangeEvent<Object>>(OnAssetChanged);

            _fsmView.OnNothingSelected = OnNothingSelected;
            _fsmView.OnObjectSelected = OnObjectSelected;
            _fsmView.OnRequestWorldPosition = GetWorldPosition;
            _fsmView.OnRequestLocalPosition = GetLocalPosition;
        }

        private void OnAssetChanged(ChangeEvent<Object> evt) {
            if (_fsmView == null) return;
            
            var obj = evt.newValue;
            var stateMachine = obj as StateMachine;
            
            if (stateMachine == null) _fsmView.ClearView();
            else _fsmView.PopulateViewFromAsset(stateMachine);
        }

        private void OnObjectSelected(Object obj) {
            _inspectorView.UpdateSelection(obj);
            if (_inspectorObject != null) {
                _inspectorObject.text = $"{obj.name} ({obj.GetType().Name})";
            }
        }
        
        private void OnNothingSelected() {
            _inspectorView.ClearInspector();
            if (_inspectorObject != null) {
                _inspectorObject.text = "";
            }
        }
        
        private Vector2 GetWorldPosition(Vector2 mousePosition) {
            var local = mousePosition - position.position;
            var world = rootVisualElement.LocalToWorld(local);
            return rootVisualElement.parent.WorldToLocal(world);
        }
        
        private Vector2 GetLocalPosition(Vector2 worldPosition) {
            var world = rootVisualElement.parent.LocalToWorld(worldPosition);
            var local = rootVisualElement.WorldToLocal(world);
            return local + position.position;
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange mode) {
            switch (mode) {
                case PlayModeStateChange.EnteredEditMode:
                    ClearViewIfWasPopulatedFromRunner();
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
            }
        }

        private void OnSelectionChange() {
            if (_fsmView == null) return;
            if (_fsmView.IsAssetDestroyed()) ClearView();
            
            if (Application.isPlaying) {
                var activeGameObject = Selection.activeGameObject;
                if (activeGameObject == null) return;

                var runner = activeGameObject.GetComponent<StateMachineRunner>();
                if (runner == null) return;

                PopulateFromRunner(runner);
            }
        }

        private void OpenSelectedObject() {
            var stateMachine = Selection.activeObject as StateMachine;
            if (stateMachine != null) PopulateFromAsset(stateMachine);
        }

        private void PopulateFromAsset(StateMachine stateMachine) {
            if (_fsmView == null) return;
            _assetPicker.value = stateMachine;
            _assetPicker.label = "Asset";
            _fsmView.PopulateViewFromAsset(stateMachine);
        }
        
        private void PopulateFromRunner(StateMachineRunner runner) {
            if (_fsmView == null) return;
            _assetPicker.value = runner.Instance;
            _assetPicker.label = "Asset (Read only)";
            _fsmView.PopulateViewFromRunner(runner);
        }
        
        private void ClearView() {
            if (_fsmView == null) return;
            _assetPicker.value = null;
            _assetPicker.label = "Asset";
            _fsmView.ClearView();
        }
        
        private void ClearViewIfWasPopulatedFromRunner() {
            if (_fsmView == null) return;
            if (_fsmView.editMode == FsmView.EditMode.RunnerInstance) {
                ClearView();
            }
        }

    }
    
}