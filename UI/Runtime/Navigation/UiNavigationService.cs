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

        public event Action<GameObject, IUiWindow> onSelectedGameObjectChanged = delegate { };
        public GameObject SelectedGameObject { get; private set; }
        public IUiWindow SelectionParent { get; private set; }
        public bool HasSelectedGameObject => _selectedGameObjectHash != 0;
        
        private IUiWindowService _uiWindowService;
        private UiNavigationSettings _settings;
        
        private readonly MultiValueDictionary<IUiWindow, IUiNavigationCallback> _windowCallbackMap = new();
        private readonly List<IUiNavigationCallback> _callbacksBuffer = new();

        private Selectable _selectable;
        private InputAction _moveInput;
        private int _selectedGameObjectHash;
        private int _lastMoveInputDir;
        private float _selectNextElementTime;
        
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
            
            // Skip disabling Unity navigation for selectables out of any IUIWindow
            if (parent == null) return;
            
            var nav = _selectable.navigation;
            
            // Disable Unity navigation
            nav.mode = UnityEngine.UI.Navigation.Mode.None;

            _selectable.navigation = nav;
        }

        private void ProcessSelectableNavigation(Selectable selectable, IUiWindow parent) {
            var moveVector = _moveInput.ReadValue<Vector2>();

            if (moveVector == default || 
                parent == null || selectable == null) 
            {
                _lastMoveInputDir = 0;
                return;
            }
            
            // Up = 1, down = -1, left = -2, right = 2
            (int dir, float mag) = Mathf.Abs(moveVector.y) >= Mathf.Abs(moveVector.x) 
                ? ((int) Mathf.Sign(moveVector.y), moveVector.y)
                : ((int) Mathf.Sign(moveVector.x) * 2f, moveVector.x);
            
            var nextElement = dir switch {
                1 => selectable.navigation.selectOnUp,
                -1 => selectable.navigation.selectOnDown,
                2 => selectable.navigation.selectOnRight,
                -2 => selectable.navigation.selectOnLeft,
                _ => null,
            };

            if (nextElement == null) {
                MoveSelectionToWindow(parent);
                return;
            }
            
            float time = Time.realtimeSinceStartup;
            
            if (_lastMoveInputDir == 0 || _lastMoveInputDir != dir) {
                // Immediate select if input was zero
                if (_lastMoveInputDir == 0) {
                    SelectGameObject(nextElement.gameObject);
                }
                
                _selectNextElementTime = time + _settings.startSelectDelay;
                _lastMoveInputDir = dir;
                return;
            }

            if (time < _selectNextElementTime) return;
            
            SelectGameObject(nextElement.gameObject);
            
            float t = _settings.selectNextDelayCurve.Evaluate(mag);
            float delay = Mathf.Lerp(_settings.selectNextDelay0, _settings.selectNextDelay1, t);
            
            _selectNextElementTime = time + delay;
        }

        private void MoveSelectionToWindow(IUiWindow window) {
            if (window == null) return;
            
            // todo iterate opened windows and their current selectable to select where to move next
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
    }
    
}