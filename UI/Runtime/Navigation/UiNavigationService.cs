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

        public event Action<Selectable, IUiWindow> OnSelectableChanged = delegate { };
        public event Action OnNavigationHierarchyChanged = delegate { };
        
        public bool HasSelectedGameObject => _selectedGameObjectHash != 0;
        public Selectable CurrentSelectable { get; private set; }
        public Selectable LastNonNullSelectable => CurrentSelectable != null ? CurrentSelectable : _selectableMap.GetValueOrDefault(_lastNonNullSelectableHash);
        
        public UiNavigationOptions SelectedObjectOptions => _selectableDataMap.GetValueOrDefault(_selectedSelectableHash).options;
        public UiNavigationMask SelectedObjectNavigationMask => _selectableDataMap.GetValueOrDefault(_selectedSelectableHash).mask;
        public GameObject SelectedGameObject { get; private set; }
        public IUiWindow SelectedGameObjectWindow { get; private set; }

        public IReadOnlyCollection<Selectable> Selectables => _selectableMap.Values;
        public IReadOnlyCollection<IUiNavigationNode> Nodes => _gameObjectIdToNodeMap.Values;
        public IReadOnlyCollection<RectTransform> ScrollableViewports => _scrollableViewports.Values;
        
        private IUiWindowService _uiWindowService;
        private UiNavigationSettings _settings;
        
        private readonly MultiValueDictionary<IUiWindow, IUiNavigationCallback> _windowCallbackMap = new();
        private readonly List<IUiNavigationCallback> _windowCallbacksBuffer = new();

        private readonly Dictionary<int, IUiNavigationNode> _gameObjectIdToNodeMap = new();
        private readonly Dictionary<int, RectTransform> _scrollableViewports = new();
        private readonly Dictionary<int, int> _childNodeToParentMap = new();
        private readonly Dictionary<int, Selectable> _selectableMap = new();
        private readonly Dictionary<int, (UiNavigationMask mask, UiNavigationOptions options)> _selectableDataMap = new();
        private readonly HashSet<int> _pauseBlockers = new();

        private InputAction _moveInput;
        private InputAction _cancelInput;
        private int _selectedGameObjectHash;
        private int _selectedSelectableHash;
        private int _lastNonNullSelectableHash;
        private int _navigateBackPerformFrame;
        private float _lastRealtimeUsedBuiltInNavigation;
        private UiNavigationDirection _lastDirUsedBuiltInNavigation;

        private int _lastSelectableHashUsedOuterNavigation;
        private float _lastRealtimeUsedOuterNavigation;
        private UiNavigationDirection _lastDirUsedOuterNavigation;

        public void Initialize(IUiWindowService uiWindowService, UiNavigationSettings settings) {
            _uiWindowService = uiWindowService;
            _settings = settings;
            
            _uiWindowService.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;
            
            _moveInput = _settings.moveInput.Get();
            _cancelInput = _settings.cancelInput.Get();
            
            _cancelInput.performed += OnCancelInputPerformed;
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        public void Dispose() {
            _uiWindowService.OnWindowsHierarchyChanged -= OnWindowsHierarchyChanged;
            
            _cancelInput.performed -= OnCancelInputPerformed;
            
            _windowCallbackMap.Clear();
            _windowCallbacksBuffer.Clear();
            
            _gameObjectIdToNodeMap.Clear();
            _childNodeToParentMap.Clear();
            _selectableMap.Clear();
            _selectableDataMap.Clear();
            _pauseBlockers.Clear();
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            ProcessSelectableNavigation(CurrentSelectable);
            CheckCurrentSelectedGameObject();
        }

        private void OnWindowsHierarchyChanged() {
            var selectable = _uiWindowService.GetFocusedWindow()?.CurrentSelected;
            if (selectable == null) return;

            SetCurrentSelectable(selectable);
        }
        
        private void CheckCurrentSelectedGameObject() {
            SelectedGameObject = EventSystem.current.currentSelectedGameObject;
            
            bool isNull = SelectedGameObject == null;
            int hash = isNull ? 0 : SelectedGameObject.GetHashCode();
            
            if (hash == _selectedGameObjectHash) return;

            _selectedGameObjectHash = hash;
            CurrentSelectable = isNull ? null : SelectedGameObject.GetComponent<Selectable>();

            if (CurrentSelectable == null) {
                _selectedSelectableHash = 0;
            }
            else {
                _selectedSelectableHash = CurrentSelectable.GetHashCode();
                _lastNonNullSelectableHash = _selectedSelectableHash;
            }

            SelectedGameObjectWindow = _uiWindowService.FindClosestParentWindow(SelectedGameObject, includeSelf: true);
            
            OnSelectableChanged.Invoke(CurrentSelectable, SelectedGameObjectWindow);
        }

        private void ProcessSelectableNavigation(Selectable selectable) {
            var moveVector = _moveInput.ReadValue<Vector2>();

            if (moveVector == default || 
                // Try to get last nonnull selectable to avoid getting stuck
                selectable == null && !_selectableMap.TryGetValue(_lastNonNullSelectableHash, out selectable)) 
            {
                // Not moving or no selectable: reset outer navigation cooldown and restore default navigation
                if (_lastRealtimeUsedOuterNavigation >= 0f) {
                    ActualizeSelectableNavigation(_lastSelectableHashUsedOuterNavigation);
                }

                _lastRealtimeUsedBuiltInNavigation = -1f;
                _lastRealtimeUsedOuterNavigation = -1f;
                return;
            }
            
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

            float time = Time.realtimeSinceStartup;

            // Outer navigation selectable or dir changed: reset outer navigation cooldown and restore default navigation
            if (_lastRealtimeUsedOuterNavigation >= 0f &&
                (_lastDirUsedOuterNavigation != dir ||
                 time > _lastRealtimeUsedOuterNavigation + _settings.outerNodeNavigationCooldown ||
                 selectable.GetHashCode() != _lastSelectableHashUsedOuterNavigation)) 
            {
                _lastRealtimeUsedOuterNavigation = -1f;
                ActualizeSelectableNavigation(_lastSelectableHashUsedOuterNavigation);
            }
            
            // Use built-in navigation
            if (nextElement != null) {
                _lastRealtimeUsedBuiltInNavigation = time;
                _lastDirUsedBuiltInNavigation = dir;
                
                // Fallback from disabled selectable
                if (!selectable.interactable || !selectable.enabled) {
                    nextElement.Select();
                }
                
                return;
            }
            
            // Cooldown for outer node navigation if same dir used
            if (dir == _lastDirUsedBuiltInNavigation &&
                _lastRealtimeUsedBuiltInNavigation >= 0f &&
                time < _lastRealtimeUsedBuiltInNavigation + _settings.outerNodeNavigationCooldown) 
            {
                return;
            }
            
            GetParentNavigationNode(selectable)?.OnNavigateOut(selectable, dir);
        }

        public void NavigateOutTo(Selectable selectable, UiNavigationDirection direction) {
            if (selectable == null) return;

            _lastDirUsedOuterNavigation = direction;
            _lastRealtimeUsedOuterNavigation = Time.realtimeSinceStartup;
            _lastSelectableHashUsedOuterNavigation = selectable.GetHashCode();
            
            var nav = selectable.navigation;
            
            switch (direction) {
                case UiNavigationDirection.Up:
                    nav.selectOnUp = null;
                    break;
                
                case UiNavigationDirection.Down:
                    nav.selectOnDown = null;
                    break;
                
                case UiNavigationDirection.Left:
                    nav.selectOnLeft = null;
                    break;
                
                case UiNavigationDirection.Right:
                    nav.selectOnRight = null;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            selectable.navigation = nav;
            
            SetCurrentSelectable(selectable);
        }

        public void SetCurrentSelectable(Selectable selectable) {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }

        public bool IsExitToPauseBlocked() {
            return _pauseBlockers.Count > 0 || NavigateBackPerformedThisFrame();
        }

        public void BlockExitToPause(object source) {
            _pauseBlockers.Add(source.GetHashCode());
        }

        public void UnblockExitToPause(object source) {
            _pauseBlockers.Remove(source.GetHashCode());
        }

        private void OnCancelInputPerformed(InputAction.CallbackContext obj) {
            OnCancelInputFromWindow();
        }

        public bool NavigateBackPerformedThisFrame() {
            return Time.frameCount == _navigateBackPerformFrame;
        }

        public void NavigateBack() {
            OnCancelInputFromWindow();
        }

        private void OnCancelInputFromWindow() {
            var window = _uiWindowService.GetFocusedWindow();
            if (window == null) return;

            var callbacks = CreateWindowCallbacksBuffer(window);
            bool canNavigateBack = true;

            for (int i = 0; i < callbacks.Count; i++) {
                canNavigateBack &= callbacks[i].CanNavigateBack();
            }

            if (!canNavigateBack) return;
            
            _navigateBackPerformFrame = Time.frameCount;
            
            _uiWindowService.SetWindowState(window, UiWindowState.Closed);
            
            for (int i = 0; i < callbacks.Count; i++) {
                callbacks[i].OnNavigateBack();
            }
        }

        public void AddWindowNavigationCallback(IUiWindow window, IUiNavigationCallback callback) {
            _windowCallbackMap.AddValue(window, callback);
        }

        public void RemoveWindowNavigationCallback(IUiWindow window, IUiNavigationCallback callback) {
            _windowCallbackMap.RemoveValue(window, callback);
        }

        private IReadOnlyList<IUiNavigationCallback> CreateWindowCallbacksBuffer(IUiWindow window) {
            _windowCallbacksBuffer.Clear();

            int count = _windowCallbackMap.GetCount(window);
            for (int i = 0; i < count; i++) {
                _windowCallbacksBuffer.Add(_windowCallbackMap.GetValueAt(window, i));
            }
            
            return _windowCallbacksBuffer;
        }
        
        public void BindNavigation(IUiNavigationNode node) {
            if (!_gameObjectIdToNodeMap.TryAdd(node.GameObject.GetHashCode(), node)) return;

            BindNavigationNode(node);
            ActualizeNavigationTree(node.GameObject);
            ActualizeSelectablesNavigation(node.GameObject);
            
            OnNavigationHierarchyChanged.Invoke();
        }

        public void UnbindNavigation(IUiNavigationNode node) {
            if (!_gameObjectIdToNodeMap.Remove(node.GameObject.GetHashCode())) return;

            UnbindNavigationNode(node);
            ActualizeNavigationTree(node.GameObject);
            ActualizeSelectablesNavigation(node.GameObject);
            
            OnNavigationHierarchyChanged.Invoke();
        }

        public void BindNavigation(Selectable selectable, UiNavigationMask mask = ~UiNavigationMask.None, UiNavigationOptions options = default) {
            int hash = selectable.GetHashCode();
            
            if (!_selectableMap.TryAdd(hash, selectable) && 
                _selectableDataMap.TryGetValue(hash, out var d) && d.mask == mask && d.options == options) 
            {
                return;
            }
            
            _selectableDataMap[hash] = (mask, options);
            BindNavigationNodeSelectable(selectable, mask);
        }

        public void UnbindNavigation(Selectable selectable) {
            int hash = selectable.GetHashCode();
            _selectableDataMap.Remove(hash);
            
            if (!_selectableMap.Remove(hash)) return;
            
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

            if (node.IsScrollable) _scrollableViewports[nodeId] = node.Viewport;
            else _scrollableViewports.Remove(nodeId);
        }
        
        private void UnbindNavigationNode(IUiNavigationNode node) {
            if (node?.GameObject == null) return;

            int nodeId = node.GameObject.GetHashCode();

            _scrollableViewports.Remove(nodeId);
            _childNodeToParentMap.Remove(nodeId);
        }

        private void BindNavigationNodeSelectable(Selectable selectable, UiNavigationMask mask) {
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
            
            parentNode.Bind(selectable, mask);
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

        public IUiNavigationNode GetNavigationNode(GameObject gameObject) {
            return _gameObjectIdToNodeMap.GetValueOrDefault(gameObject.GetHashCode());
        }

        public IUiNavigationNode GetParentNavigationNode(GameObject gameObject) {
            return _gameObjectIdToNodeMap.GetValueOrDefault(_childNodeToParentMap.GetValueOrDefault(gameObject.GetHashCode()));
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
                var data = _selectableDataMap.GetValueOrDefault(selectable.GetHashCode());
                BindNavigationNodeSelectable(selectable, data.mask);
            }
        }
        
        private void ActualizeSelectableNavigation(int selectableHash) {
            if (!_selectableMap.TryGetValue(selectableHash, out var selectable)) return;

            BindNavigationNodeSelectable(selectable, _selectableDataMap.GetValueOrDefault(selectableHash).mask);
        }
    }
    
}