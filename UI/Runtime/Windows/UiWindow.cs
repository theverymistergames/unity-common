using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Windows {
    
    public sealed class UiWindow : MonoBehaviour, IUiWindow {
        
        [SerializeField] private WindowState _initialState;
        [SerializeField] private int _layer;
        [SerializeField] private Selectable _firstSelected;
        [SerializeField] private GameObject[] _enableGameObjects;
        [SerializeField] private Child[] _children;

        private enum WindowState {
            Root,
            Opened,
            Closed,
        }
        
        [Serializable]
        private struct Child {
            public UiWindow window;
            public UiWindowMode mode;
        }

        public GameObject GameObject => gameObject;
        public int Layer => _layer;
        public bool IsRoot => _initialState == WindowState.Root;
        public UiWindowState State { get; private set; }
        public bool IsFocused { get; private set; }
        public GameObject CurrentSelectable { get; private set; }

        private void Awake() {
            CurrentSelectable = _firstSelected.gameObject;
            
            RegisterWindow();
        }

        private void OnDestroy() {
            UnregisterWindow();
            UnsubscribeSelectedGameObject();
        }

        private void RegisterWindow() {
            var windowService = Services.Get<IUiWindowService>();
            
            windowService.RegisterWindow(this);
            
            for (int i = 0; i < _children.Length; i++) {
                ref var child = ref _children[i];
                
                windowService.RegisterWindow(child.window);
                windowService.RegisterRelation(this, child.window, child.mode);
            }

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
            if (Services.Get<IUiWindowService>() is not { } service) return;
            
            for (int i = 0; i < _children.Length; i++) {
                ref var child = ref _children[i];
                
                service.UnregisterRelation(this, child.window);
                service.UnregisterWindow(child.window);
            }
            
            service.UnregisterWindow(this);
        }

        private void SubscribeSelectedGameObject() {
            if (Services.Get<IUiNavigationService>() is not { } service) return;
            
            service.onSelectedGameObjectChanged -= OnSelectedGameObjectChanged;
            service.onSelectedGameObjectChanged += OnSelectedGameObjectChanged;
        }

        private void UnsubscribeSelectedGameObject() {
            if (Services.Get<IUiNavigationService>() is not { } service) return;
            
            service.onSelectedGameObjectChanged -= OnSelectedGameObjectChanged;
        }

        private void OnSelectedGameObjectChanged(GameObject obj) {
            if (obj == null ||
                Services.Get<IUiWindowService>().GetClosestParentWindow(obj) is not { } window ||
                !ReferenceEquals(window, this)
            ) {
                CurrentSelectable = _firstSelected.gameObject;
                return;
            }

            CurrentSelectable = obj;
        }

        void IUiWindow.NotifyWindowState(UiWindowState state, bool focused) {
            State = state;
            IsFocused = focused;

            switch (state) {
                case UiWindowState.Closed:
                    _enableGameObjects.SetActive(false);
                    UnsubscribeSelectedGameObject();
                    break;
                
                case UiWindowState.Opened:
                    _enableGameObjects.SetActive(true);
                    SubscribeSelectedGameObject();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

#if UNITY_EDITOR
        [Button(mode: ButtonAttribute.Mode.Runtime)] 
        private void OpenWindow() => Services.Get<IUiWindowService>()?.SetWindowState(this, UiWindowState.Opened);
        [Button(mode: ButtonAttribute.Mode.Runtime)] 
        private void CloseWindow() => Services.Get<IUiWindowService>()?.SetWindowState(this, UiWindowState.Closed);
#endif
    }
    
}