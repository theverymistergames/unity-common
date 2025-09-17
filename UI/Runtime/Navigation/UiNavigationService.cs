using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MisterGames.UI.Navigation {
    
    public sealed class UiNavigationService : IUiNavigationService, IUpdate, IDisposable {

        public event Action<GameObject> onSelectedGameObjectChanged = delegate { };
        public GameObject SelectedGameObject { get; private set; }
        public bool HasSelectedGameObject => _selectedGameObjectHash != 0;
        
        private IUiWindowService _uiWindowService;
        private UiNavigationSettings _uiNavigationSettings;
        
        private readonly MultiValueDictionary<IUiWindow, IUiNavigationCallback> _windowCallbackMap = new();
        private readonly List<IUiNavigationCallback> _callbacksBuffer = new();

        private int _selectedGameObjectHash;
        
        public void Initialize(IUiWindowService uiWindowService, UiNavigationSettings settings) {
            _uiWindowService = uiWindowService;
            _uiNavigationSettings = settings;
            
            _uiWindowService.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;
            _uiNavigationSettings.cancelInput.Get().performed += OnCancelInputPerformed;
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
            
            OnWindowsHierarchyChanged();
        }

        public void Dispose() {
            _uiWindowService.OnWindowsHierarchyChanged -= OnWindowsHierarchyChanged;
            _uiNavigationSettings.cancelInput.Get().performed -= OnCancelInputPerformed;
            
            _windowCallbackMap.Clear();
            _callbacksBuffer.Clear();
            
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            CheckCurrentSelectedGameObject();
        }

        private void CheckCurrentSelectedGameObject() {
            SelectedGameObject = EventSystem.current.currentSelectedGameObject;
            
            int hash = SelectedGameObject == null ? 0 : SelectedGameObject.GetHashCode();
            if (hash == _selectedGameObjectHash) return;

            _selectedGameObjectHash = hash;
            
            FetchNavigation(SelectedGameObject);
            
            onSelectedGameObjectChanged.Invoke(SelectedGameObject);
        }

        public void SelectGameObject(GameObject gameObject) {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        private void OnWindowsHierarchyChanged() {
            var selectable = _uiWindowService.GetFrontWindow()?.CurrentSelectable;
            if (selectable == null) return;
            
            SelectGameObject(selectable);
        }

        private void FetchNavigation(GameObject gameObject) {
            if (gameObject == null) return;
            
            
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