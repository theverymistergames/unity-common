using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UiNavigationNode))]
    public sealed class UiWindow : MonoBehaviour, IUiWindow {
        
        [SerializeField] private WindowState _initialState;
        [SerializeField] private int _layer;
        [VisibleIf(nameof(_initialState), 0, CompareMode.NotEqual)]
        [SerializeField] private UiWindowMode _mode;
        [SerializeField] private GameObject[] _enableGameObjects;
        [SerializeField] private Selectable _firstSelected;

        private enum WindowState {
            Root,
            Opened,
            Closed,
        }
        
        public GameObject GameObject => gameObject;
        public GameObject CurrentSelected { get; private set; }

        public int Layer => _layer;
        public bool IsRoot => _initialState == WindowState.Root;
        public UiWindowMode Mode => _mode;
        public UiWindowState State { get; private set; }
        public bool IsFocused { get; private set; }

        private void Awake() {
            CurrentSelected = _firstSelected?.gameObject;
            
            RegisterWindow();
        }

        private void OnDestroy() {
            UnregisterWindow();
            UnsubscribeSelectedGameObject();
        }

        private void RegisterWindow() {
            var windowService = Services.Get<IUiWindowService>();
            
            windowService.RegisterWindow(this);

            var state = _initialState switch {
                WindowState.Root => UiWindowState.Opened,
                WindowState.Opened => UiWindowState.Opened,
                WindowState.Closed => UiWindowState.Closed,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            // Prevent setting initial window state if it has a parent window 
            if (windowService.GetParentWindow(this) == null) {
                windowService.SetWindowState(this, state);
            }
        }

        private void UnregisterWindow() {
            Services.Get<IUiWindowService>()?.UnregisterWindow(this);
        }

        private void SubscribeSelectedGameObject() {
            if (Services.Get<IUiNavigationService>() is not { } service) return;
            
            service.OnSelectedGameObjectChanged -= OnSelectedGameObjectChanged;
            service.OnSelectedGameObjectChanged += OnSelectedGameObjectChanged;
        }

        private void UnsubscribeSelectedGameObject() {
            if (Services.Get<IUiNavigationService>() is not { } service) return;
            
            service.OnSelectedGameObjectChanged -= OnSelectedGameObjectChanged;
        }

        private void OnSelectedGameObjectChanged(GameObject obj, IUiWindow window) {
            if (obj == null || !ReferenceEquals(window, this)) return;

            CurrentSelected = obj;
        }
        
        void IUiWindow.NotifyWindowState(UiWindowState state, bool focused) {
            State = state;
            IsFocused = focused;

            switch (state) {
                case UiWindowState.Closed:
                    _enableGameObjects.SetActive(false);
                    UnsubscribeSelectedGameObject();
                    MaybeResetSelectionToFirst();
                    break;
                
                case UiWindowState.Opened:
                    _enableGameObjects.SetActive(true);
                    SubscribeSelectedGameObject();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        private void MaybeResetSelectionToFirst() {
            // Do not reset to first if focused window is a child of this window to preserve selection history.
            if (!Services.TryGet(out IUiWindowService service) ||
                service.IsChildWindow(this, service.GetFocusedWindow()) || 
                _firstSelected == null) 
            {
                return;
            }
            
            CurrentSelected = _firstSelected?.gameObject;
        }

#if UNITY_EDITOR
        [Button(mode: ButtonAttribute.Mode.Runtime)] 
        private void OpenWindow() => Services.Get<IUiWindowService>()?.SetWindowState(this, UiWindowState.Opened);
        [Button(mode: ButtonAttribute.Mode.Runtime)] 
        private void CloseWindow() => Services.Get<IUiWindowService>()?.SetWindowState(this, UiWindowState.Closed);
#endif
    }
    
}