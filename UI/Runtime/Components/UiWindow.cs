using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.UI.Data;
using MisterGames.UI.Service;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public sealed class UiWindow : MonoBehaviour, IUiWindow {
        
        [SerializeField] private WindowState _initialState;
        [VisibleIf(nameof(_initialState), 0)]
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

        public Selectable FirstSelected => _firstSelected;
        public UiWindowState State { get; private set; }
        public bool IsOpened => gameObject is { activeSelf: true, activeInHierarchy: true };
        public bool IsRoot => _initialState == WindowState.Root;
        
        private UiWindowState _currentState;

        private void Awake() {
            if (Services.Get<IUIWindowService>() is not { } service) return;
            
            service.RegisterWindow(this, _layer);
            
            for (int i = 0; i < _children.Length; i++) {
                ref var child = ref _children[i];
                service.RegisterRelation(this, child.window, child.mode);
            }

            var state = _initialState switch {
                WindowState.Root => UiWindowState.Opened,
                WindowState.Opened => UiWindowState.Opened,
                WindowState.Closed => UiWindowState.Closed,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            service.SetWindowState(this, state);
        }

        private void OnDestroy() {
            if (Services.Get<IUIWindowService>() is not { } service) return;
            
            for (int i = 0; i < _children.Length; i++) {
                ref var child = ref _children[i];
                service.UnregisterRelation(this, child.window);
            }
            
            service.UnregisterWindow(this);
        }

        void IUiWindow.NotifyWindowState(UiWindowState state) {
            State = state;
            
            switch (state) {
                case UiWindowState.Closed:
                    _enableGameObjects.SetActive(false);
                    break;
                
                case UiWindowState.Opened:
                    _enableGameObjects.SetActive(true);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

#if UNITY_EDITOR
        [Button] private void OpenWindow() => Services.Get<IUIWindowService>().SetWindowState(this, UiWindowState.Opened);
        [Button] private void CloseWindow() => Services.Get<IUIWindowService>().SetWindowState(this, UiWindowState.Closed);
#endif
    }
    
}