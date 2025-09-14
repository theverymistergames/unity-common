using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Input.Actions;
using MisterGames.UI.Windows;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MisterGames.UI.Navigation {
    
    public sealed class UiNavigationService : IUiNavigationService, IDisposable {

        private IUiWindowService _uiWindowService;
        private UiNavigationSettings _uiNavigationSettings;
        
        private readonly MultiValueDictionary<IUiWindow, IUiNavigationCallback> _windowCallbackMap = new();
        private readonly List<IUiNavigationCallback> _callbacksBuffer = new();
        
        public void Initialize(IUiWindowService uiWindowService, UiNavigationSettings settings) {
            _uiWindowService = uiWindowService;
            _uiNavigationSettings = settings;
            
            _uiWindowService.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;
            _uiNavigationSettings.cancelInput.Get().performed += OnCancelInputPerformed;
        }

        public void Dispose() {
            _uiWindowService.OnWindowsHierarchyChanged -= OnWindowsHierarchyChanged;
            _uiNavigationSettings.cancelInput.Get().performed -= OnCancelInputPerformed;
            
            _windowCallbackMap.Clear();
            _callbacksBuffer.Clear();
        }

        private void OnWindowsHierarchyChanged() {
            var frontWindow = _uiWindowService.GetFrontWindow();
            var selectable = frontWindow?.FirstSelectable;
            
            if (selectable == null) return;
            
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
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