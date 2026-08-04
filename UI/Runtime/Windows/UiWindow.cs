using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.UI.Navigation {
    
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UiNavigationNode))]
    public sealed class UiWindow : MonoBehaviour, IUiWindow {
        
        [Header("Window")]
        [SerializeField] private int _layer;
        [SerializeField] private UiWindowState _state;
        [SerializeField] private UiWindowOpenMode _openMode;
        [SerializeField] private UiWindowCloseMode _closeMode;
        [SerializeField] private UiWindowOptions _options;
        
        [Header("View")]
        [FormerlySerializedAs("_enableGameObjects")]
        [SerializeField] private GameObject[] _enableOnWindowOpened;
        [SerializeField] private GameObject[] _enableOnBranchOpened;

        public event Action<UiWindowState> OnBeforeStateChanged = delegate { };
        public event Action<UiWindowState> OnAfterStateChanged = delegate { };

        public GameObject GameObject => gameObject;
        public int Layer => _layer;
        public UiWindowOpenMode OpenMode => _openMode;
        public UiWindowCloseMode CloseMode => _closeMode;
        public UiWindowState State => _state;
        public UiWindowOptions Options => _options;
        public IUiNavigationNode Node => _node ?? GetComponent<IUiNavigationNode>();

        private IUiWindowService _windowService;
        private IUiNavigationNode _node;
        
        private void Awake() {
            _windowService = Services.Get<IUiWindowService>();
            _windowService.RegisterWindow(this, _state);
        }

        private void OnDestroy() {
            _windowService.UnregisterWindow(this);
        }

        private void OnEnable() {
            _windowService.NotifyWindowEnabled(this, true);
        }

        private void OnDisable() {
            _windowService.NotifyWindowEnabled(this, false);
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
            OnBeforeStateChanged.Invoke(_state);
            
            SetEnableState(_enableOnBranchOpened, _windowService.IsInOpenedBranch(this));
            
            switch (state) {
                case UiWindowState.Closed:
                    SetEnableState(_enableOnWindowOpened, false);
                    break;
                
                case UiWindowState.Opened:
                    SetEnableState(_enableOnWindowOpened, true);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            
            OnAfterStateChanged.Invoke(_state);
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
        [Button] 
        private void OpenWindow() {
            if (Application.isPlaying) {
                _windowService.SetWindowState(this, UiWindowState.Opened);
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
                _windowService.SetWindowState(this, UiWindowState.Closed);
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