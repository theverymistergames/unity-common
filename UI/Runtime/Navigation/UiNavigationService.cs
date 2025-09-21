using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public sealed class UiNavigationService : IUiNavigationService, IUpdate, IDisposable {

        public event Action<GameObject, IUiWindow> OnSelectedGameObjectChanged = delegate { };

        public GameObject SelectedGameObject { get; private set; }
        public IUiWindow SelectedGameObjectWindow { get; private set; }
        public bool HasSelectedGameObject => _selectedGameObjectHash != 0;

        public IReadOnlyCollection<Selectable> Selectables => _selectableMap.Values;
        
        private IUiWindowService _uiWindowService;
        private UiNavigationSettings _settings;
        
        private readonly MultiValueDictionary<IUiWindow, IUiNavigationCallback> _windowCallbackMap = new();
        private readonly List<IUiNavigationCallback> _callbacksBuffer = new();

        private readonly Dictionary<int, IUiNavigationNode> _gameObjectIdToNodeMap = new();
        private readonly Dictionary<int, int> _childNodeToParentMap = new();
        private readonly Dictionary<int, Selectable> _selectableMap = new();

        private Selectable _selectable;
        private InputAction _moveInput;
        private int _selectedGameObjectHash;
        
        public void Initialize(IUiWindowService uiWindowService, UiNavigationSettings settings) {
            _uiWindowService = uiWindowService;
            _settings = settings;
            
            _uiWindowService.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;
            
            _settings.cancelInput.Get().performed += OnCancelInputPerformed;
            _moveInput = _settings.moveInput.Get();
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        public void Dispose() {
            _uiWindowService.OnWindowsHierarchyChanged -= OnWindowsHierarchyChanged;
            
            _settings.cancelInput.Get().performed -= OnCancelInputPerformed;
            
            _windowCallbackMap.Clear();
            _callbacksBuffer.Clear();
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            CheckCurrentSelectedGameObject();
            ProcessSelectableNavigation(_selectable);
        }

        private void OnWindowsHierarchyChanged() {
            var selectable = _uiWindowService.GetFocusedWindow()?.CurrentSelected;
            if (selectable == null) return;

            SelectGameObject(selectable);
        }
        
        private void CheckCurrentSelectedGameObject() {
            SelectedGameObject = EventSystem.current.currentSelectedGameObject;
            
            int hash = SelectedGameObject == null ? 0 : SelectedGameObject.GetHashCode();
            if (hash == _selectedGameObjectHash) return;

            _selectedGameObjectHash = hash;
            SelectedGameObjectWindow = _uiWindowService.FindClosestParentWindow(SelectedGameObject, includeSelf: true);
            
            FetchNavigation(SelectedGameObject);
            
            OnSelectedGameObjectChanged.Invoke(SelectedGameObject, SelectedGameObjectWindow);
        }

        private void FetchNavigation(GameObject gameObject) {
            if (gameObject == null || 
                !gameObject.TryGetComponent(out Selectable selectable)) 
            {
                _selectable = null;
                return;
            }

            _selectable = selectable;
        }

        private void ProcessSelectableNavigation(Selectable selectable) {
            var moveVector = _moveInput.ReadValue<Vector2>();
            if (moveVector == default || selectable == null) return;
            
            var dir = Mathf.Abs(moveVector.y) >= Mathf.Abs(moveVector.x) 
                ? Mathf.Sign(moveVector.y) > 0f ? UiNavigationDirection.Up : UiNavigationDirection.Down
                : Mathf.Sign(moveVector.x) > 0f ? UiNavigationDirection.Right : UiNavigationDirection.Left;
            
            var nextElement = dir switch {
                UiNavigationDirection.Up => selectable.navigation.selectOnUp,
                UiNavigationDirection.Down => selectable.navigation.selectOnDown,
                UiNavigationDirection.Right => selectable.navigation.selectOnRight,
                UiNavigationDirection.Left => selectable.navigation.selectOnLeft,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (nextElement == null) {
                GetParentNavigationNode(selectable)?.OnNavigateOut(selectable, dir);
            }
        }

        public void SelectGameObject(GameObject gameObject) {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        private void OnCancelInputPerformed(InputAction.CallbackContext obj) {
            PerformCancel();
        }

        public void PerformCancel() {
            var frontWindow = _uiWindowService.GetFocusedWindow();
            if (frontWindow == null) return;

            var callbacks = CreateCallbacksBuffer(frontWindow);
            bool canCloseFrontWindow = true;

            for (int i = 0; i < callbacks.Count; i++) {
                canCloseFrontWindow &= callbacks[i].OnNavigateBack();
            }

            if (canCloseFrontWindow) {
                _uiWindowService.SetWindowState(frontWindow, UiWindowState.Closed);
            }
        }

        public void AddNavigationCallback(IUiWindow window, IUiNavigationCallback callback) {
            _windowCallbackMap.AddValue(window, callback);
        }

        public void RemoveNavigationCallback(IUiWindow window, IUiNavigationCallback callback) {
            _windowCallbackMap.RemoveValue(window, callback);
        }

        private IReadOnlyList<IUiNavigationCallback> CreateCallbacksBuffer(IUiWindow window) {
            _callbacksBuffer.Clear();

            int count = _windowCallbackMap.GetCount(window);
            for (int i = 0; i < count; i++) {
                _callbacksBuffer.Add(_windowCallbackMap.GetValueAt(window, i));
            }
            
            return _callbacksBuffer;
        }
        
        public void BindNavigation(IUiNavigationNode node) {
            if (!_gameObjectIdToNodeMap.TryAdd(node.GameObject.GetHashCode(), node)) return;
            
            BindNavigationNode(node);
            ActualizeNavigationTree(node.GameObject);
            ActualizeSelectablesNavigation(node.GameObject);
        }

        public void UnbindNavigation(IUiNavigationNode node) {
            if (!_gameObjectIdToNodeMap.Remove(node.GameObject.GetHashCode())) return;
            
            UnbindNavigationNode(node);
            ActualizeNavigationTree(node.GameObject);
            ActualizeSelectablesNavigation(node.GameObject);
        }

        public void BindNavigation(Selectable selectable) {
            if (!_selectableMap.TryAdd(selectable.GetHashCode(), selectable)) return;
            
            BindNavigationNodeSelectable(selectable);
        }

        public void UnbindNavigation(Selectable selectable) {
            if (!_selectableMap.Remove(selectable.GetHashCode())) return;
            
            UnbindNavigationNodeSelectable(selectable);
        }

        private void BindNavigationNode(IUiNavigationNode node) {
            if (node?.GameObject == null) return;
            
            var parentNode = FindClosestParentNavigationNode(node.GameObject, includeSelf: false);
            
            if (parentNode == null) {
                UnbindNavigationNode(node);
                return;
            }
            
            int nodeId = node.GameObject.GetHashCode();
            int parentNodeId = parentNode.GameObject.GetHashCode();
            
            if (_childNodeToParentMap.TryGetValue(nodeId, out int existentNodeId) && existentNodeId != parentNodeId) {
                UnbindNavigationNode(node);
            }
            
            _childNodeToParentMap[nodeId] = parentNodeId;
            
            //parentNode.Bind(node);
        }
        
        private void UnbindNavigationNode(IUiNavigationNode node) {
            if (node?.GameObject == null ||
                !_childNodeToParentMap.Remove(node.GameObject.GetHashCode(), out int parentNodeId) || 
                !_gameObjectIdToNodeMap.TryGetValue(parentNodeId, out var parentNode)) 
            {
                return;
            }
            
            //parentNode.Unbind(node);
        }

        private void BindNavigationNodeSelectable(Selectable selectable) {
            if (selectable == null) return;
            
            var parentNode = FindClosestParentNavigationNode(selectable.gameObject, includeSelf: true);
            
            if (parentNode == null) {
                UnbindNavigationNodeSelectable(selectable);
                return;
            }
            
            int selectableId = selectable.GetHashCode();
            int parentNodeId = parentNode.GameObject.GetHashCode();
            
            if (_childNodeToParentMap.TryGetValue(selectableId, out int existentNodeId) && existentNodeId != parentNodeId) {
                UnbindNavigationNodeSelectable(selectable);
            }
            
            _childNodeToParentMap[selectableId] = parentNodeId;
            
            parentNode.Bind(selectable);
            parentNode.UpdateNavigation();
        }
        
        private void UnbindNavigationNodeSelectable(Selectable selectable) {
            if (selectable == null ||
                !_childNodeToParentMap.Remove(selectable.GetHashCode(), out int parentNodeId) || 
                !_gameObjectIdToNodeMap.TryGetValue(parentNodeId, out var parentNode)) 
            {
                return;
            }
            
            parentNode.Unbind(selectable);
            parentNode.UpdateNavigation();
        }

        public IUiNavigationNode GetParentNavigationNode(Selectable selectable) {
            return _gameObjectIdToNodeMap.GetValueOrDefault(_childNodeToParentMap.GetValueOrDefault(selectable.GetHashCode()));
        }

        public IUiNavigationNode GetParentNavigationNode(IUiNavigationNode node) {
            return _gameObjectIdToNodeMap.GetValueOrDefault(_childNodeToParentMap.GetValueOrDefault(node.GameObject.GetHashCode()));
        }

        public IUiNavigationNode FindClosestParentNavigationNode(GameObject gameObject, bool includeSelf = true) {
            if (!includeSelf) gameObject = gameObject?.transform.parent?.gameObject;
            
            while (gameObject != null) {
                if (_gameObjectIdToNodeMap.TryGetValue(gameObject.GetHashCode(), out var node)) return node;

                gameObject = gameObject.transform.parent?.gameObject;
            }
            
            return null;
        }
        
        public bool IsChildNode(IUiNavigationNode node, IUiNavigationNode parent, bool direct) {
            return node?.GameObject != null && parent?.GameObject != null &&
                   (direct 
                       ? GetParentNavigationNode(node) == parent 
                       : node.GameObject.transform.IsChildOf(parent.GameObject.transform));
        }

        private void ActualizeNavigationTree(GameObject root) {
            if (root == null) return;
            
            var rootTrf = root.transform;
            
            foreach (var node in _gameObjectIdToNodeMap.Values) {
                if (!node.GameObject.transform.IsChildOf(rootTrf)) continue;
                
                BindNavigationNode(node);
            }
        }
        
        private void ActualizeSelectablesNavigation(GameObject root) {
            if (root == null) return;
            
            var rootTrf = root.transform;
            
            foreach (var selectable in _selectableMap.Values) {
                if (!selectable.transform.IsChildOf(rootTrf)) continue;
                
                BindNavigationNodeSelectable(selectable);
            }
        }
    }
    
}