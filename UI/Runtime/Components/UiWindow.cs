using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.UI.Components {
    
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UiNavigationNode))]
    public sealed class UiWindow : MonoBehaviour, IUiWindow {
        
        [Header("Window")]
        [SerializeField] private int _layer;
        [SerializeField] private UiWindowState _state;
        [SerializeField] private UiWindowOpenMode _openMode;
        [SerializeField] private UiWindowCloseMode _closeMode;
        [SerializeField] private UiWindowOptions _options;
        
        [Header("Selection")]
        [SerializeField] private Selectable _firstSelected;
        
        [Header("View")]
        [FormerlySerializedAs("_enableGameObjects")]
        [SerializeField] private GameObject[] _enableOnWindowOpened;
        [SerializeField] private GameObject[] _enableOnBranchOpened;
        
        public GameObject GameObject => gameObject;
        public GameObject CurrentSelected { get; private set; }
        
        public int Layer => _layer;
        public UiWindowOpenMode OpenMode => _openMode;
        public UiWindowCloseMode CloseMode => _closeMode;
        public UiWindowState State => _state;
        public UiWindowOptions Options => _options;

        private void Awake() {
            CurrentSelected = _firstSelected?.gameObject;
            
            Services.Get<IUiWindowService>()?.RegisterWindow(this, _state);
        }

        private void OnDestroy() {
            Services.Get<IUiWindowService>()?.UnregisterWindow(this);
            UnsubscribeSelectedGameObject();
        }

        private void OnEnable() {
            Services.Get<IUiWindowService>()?.NotifyWindowEnabled(this, true);
        }

        private void OnDisable() {
            Services.Get<IUiWindowService>()?.NotifyWindowEnabled(this, false);
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
        
        void IUiWindow.NotifyWindowState(UiWindowState state) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                SetEnableState(_enableOnWindowOpened, state == UiWindowState.Opened);
                SetEnableState(_enableOnBranchOpened, state == UiWindowState.Opened);
                return;
            }
#endif
            
            _state = state;
            
            SetEnableState(_enableOnBranchOpened, Services.Get<IUiWindowService>().IsInOpenedBranch(this));
            
            switch (state) {
                case UiWindowState.Closed:
                    SetEnableState(_enableOnWindowOpened, false);
                    UnsubscribeSelectedGameObject();
                    MaybeResetSelectionToFirst();
                    break;
                
                case UiWindowState.Opened:
                    SetEnableState(_enableOnWindowOpened, true);
                    SubscribeSelectedGameObject();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void MaybeResetSelectionToFirst() {
            // Do not reset to first if focused window is a child of this window to preserve selection history.
            if (!Services.TryGet(out IUiWindowService service) ||
                _firstSelected == null ||
                service.GetRootWindow(this) is { } root && 
                service.GetFrontOpenedWindow(root.Layer) is { } front &&
                service.IsChildWindow(window: this, child: front)) 
            {
                return;
            }
            
            CurrentSelected = _firstSelected?.gameObject;
        }
        
        private static void SetEnableState(GameObject[] gameObjects, bool enable) {
            for (int i = 0; i < gameObjects?.Length; i++) {
                var go = gameObjects[i];
                
#if UNITY_EDITOR
                if (!Application.isPlaying && (go == null || go.IsEnabled() == enable)) continue;
#endif

                go.SetActive(enable);
                
#if UNITY_EDITOR
                if (!Application.isPlaying) EditorUtility.SetDirty(go);
#endif
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_state == UiWindowState.Closed) CloseWindow();
            else OpenWindow();
        }

        [Button] 
        private void OpenWindow() {
            if (Application.isPlaying) {
                Services.Get<IUiWindowService>()?.SetWindowState(this, UiWindowState.Opened);
                return;
            }

            var windows = gameObject.GetComponentsInChildren<IUiWindow>();
            
            for (int i = 0; i < windows.Length; i++) {
                var uiWindow = windows[i];
                var state = ReferenceEquals(uiWindow, this) ? UiWindowState.Opened : UiWindowState.Closed;
                uiWindow.NotifyWindowState(state);
            }
        }

        [Button] 
        private void CloseWindow() {
            if (Application.isPlaying) {
                Services.Get<IUiWindowService>()?.SetWindowState(this, UiWindowState.Closed);
                return;
            }
            
            var windows = gameObject.GetComponentsInChildren<IUiWindow>();
            
            for (int i = 0; i < windows.Length; i++) {
                windows[i].NotifyWindowState(UiWindowState.Closed);
            }
        }
#endif
    }
    
}