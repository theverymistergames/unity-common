using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using MisterGames.UI.Windows;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    public sealed class UiNavigationService : IUiNavigationService, IUpdate, IDisposable {

        public event Action<GameObject, IUiWindow> onSelectedGameObjectChanged = delegate { };
        public GameObject SelectedGameObject { get; private set; }
        public IUiWindow SelectionParent { get; private set; }
        public bool HasSelectedGameObject => _selectedGameObjectHash != 0;
        
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
            
            OnWindowsHierarchyChanged();
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
            ProcessSelectableNavigation(_selectable, SelectionParent);
        }

        private void CheckCurrentSelectedGameObject() {
            SelectedGameObject = EventSystem.current.currentSelectedGameObject;
            
            int hash = SelectedGameObject == null ? 0 : SelectedGameObject.GetHashCode();
            if (hash == _selectedGameObjectHash) return;

            _selectedGameObjectHash = hash;
            SelectionParent = _uiWindowService.GetClosestParentWindow(SelectedGameObject);
            
            FetchNavigation(SelectedGameObject, SelectionParent);
            
            onSelectedGameObjectChanged.Invoke(SelectedGameObject, SelectionParent);
        }

        private void FetchNavigation(GameObject gameObject, IUiWindow parent) {
            if (gameObject == null || 
                !gameObject.TryGetComponent(out Selectable selectable)) 
            {
                _selectable = null;
                return;
            }

            _selectable = selectable;
        }

        private void ProcessSelectableNavigation(Selectable selectable, IUiWindow parent) {
            var moveVector = _moveInput.ReadValue<Vector2>();

            if (moveVector == default || 
                parent == null || selectable == null) 
            {
                return;
            }
            
            // Up = 1, down = -1, left = -2, right = 2
            int dir = Mathf.Abs(moveVector.y) >= Mathf.Abs(moveVector.x) 
                ? (int) Mathf.Sign(moveVector.y)
                : (int) (Mathf.Sign(moveVector.x) * 2f);
            
            var nextElement = dir switch {
                1 => selectable.navigation.selectOnUp,
                -1 => selectable.navigation.selectOnDown,
                2 => selectable.navigation.selectOnRight,
                -2 => selectable.navigation.selectOnLeft,
                _ => null,
            };

            if (nextElement == null) {
                MoveSelectionToWindow(parent);
            }
        }

        private void MoveSelectionToWindow(IUiWindow window) {
            if (window == null) return;

            
        }

        public void SelectGameObject(GameObject gameObject) {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        private void OnWindowsHierarchyChanged() {
            var selectable = _uiWindowService.GetFrontWindow()?.CurrentSelectable;
            if (selectable == null) return;
            
            SelectGameObject(selectable);
        }

        private void OnCancelInputPerformed(InputAction.CallbackContext obj) {
            PerformCancel();
        }

        public void PerformCancel() {
            var frontWindow = _uiWindowService.GetFrontWindow();
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
            _gameObjectIdToNodeMap[node.GameObject.GetHashCode()] = node;
            
            BindNavigationNode(node);
            ActualizeNavigationTree(node.GameObject);
            ActualizeSelectablesNavigation(node.GameObject);
        }

        public void UnbindNavigation(IUiNavigationNode node) {
            _gameObjectIdToNodeMap.Remove(node.GameObject.GetHashCode());
            
            UnbindNavigationNode(node);
            ActualizeNavigationTree(node.GameObject);
            ActualizeSelectablesNavigation(node.GameObject);
        }

        public void BindNavigation(Selectable selectable) {
            _selectableMap[selectable.GetHashCode()] = selectable;
            
            BindNavigationNodeSelectable(selectable);
        }

        public void UnbindNavigation(Selectable selectable) {
            _selectableMap.Remove(selectable.GetHashCode());
            
            UnbindNavigationNodeSelectable(selectable);
        }
        
        private void BindNavigationNode(IUiNavigationNode node) {
            if (node?.GameObject == null) return;
            
            var parentNode = GetClosestParentNavigationNode(node.GameObject);
            
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
            
            parentNode.Bind(node);
        }
        
        private void UnbindNavigationNode(IUiNavigationNode node) {
            if (node?.GameObject == null ||
                !_childNodeToParentMap.Remove(node.GameObject.GetHashCode(), out int parentNodeId) || 
                !_gameObjectIdToNodeMap.TryGetValue(parentNodeId, out var parentNode)) 
            {
                return;
            }
            
            parentNode.Unbind(node);
        }

        private void BindNavigationNodeSelectable(Selectable selectable) {
            if (selectable == null) return;
            
            var parentNode = GetClosestParentNavigationNode(selectable.gameObject);
            
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
        }
        
        private void UnbindNavigationNodeSelectable(Selectable selectable) {
            if (selectable == null ||
                !_childNodeToParentMap.Remove(selectable.GetHashCode(), out int parentNodeId) || 
                !_gameObjectIdToNodeMap.TryGetValue(parentNodeId, out var parentNode)) 
            {
                return;
            }
            
            parentNode.Unbind(selectable);
        }

        private IUiNavigationNode GetClosestParentNavigationNode(GameObject gameObject) {
            while (gameObject != null) {
                if (_gameObjectIdToNodeMap.TryGetValue(gameObject.GetHashCode(), out var node)) return node;

                gameObject = gameObject.transform.parent?.gameObject;
            }
            
            return null;
        }

        private void ActualizeNavigationTree(GameObject root) {
            var ids = new NativeArray<int>(_gameObjectIdToNodeMap.Count, Allocator.Temp);
            int count = 0;
            
            var rootTrf = root.transform;
            
            foreach ((int id, var node) in _gameObjectIdToNodeMap) {
                if (!node.GameObject.transform.IsChildOf(rootTrf)) continue;
                
                ids[count++] = id;
            }

            for (int i = 0; i < count; i++) {
                BindNavigationNode(_gameObjectIdToNodeMap[ids[i]]);
            }
            
            ids.Dispose();
        }
        
        private void ActualizeSelectablesNavigation(GameObject root) {
            var ids = new NativeArray<int>(_selectableMap.Count, Allocator.Temp);
            int count = 0;
            
            var rootTrf = root.transform;
            
            foreach ((int id, var selectable) in _selectableMap) {
                if (!selectable.transform.IsChildOf(rootTrf)) continue;
                
                ids[count++] = id;
            }

            for (int i = 0; i < count; i++) {
                BindNavigationNodeSelectable(_selectableMap[ids[i]]);
            }
            
            ids.Dispose();
        }
    }
    
}