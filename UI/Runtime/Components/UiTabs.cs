using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using MisterGames.Common.Pooling;
using MisterGames.Common.Service;
using MisterGames.UI.Data;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public sealed class UiTabs : MonoBehaviour, IUiNavigationCallback {
        
        [SerializeField] private Tab[] _tabs;
        [SerializeField] private Optional<UiElementState> _setupMinState = new(UiElementState.Hover, hasValue: false);
        [SerializeField] private RectTransform _tabSelectionPrefab;
        [SerializeField] private Transform _tabSelectionParent;
        [SerializeField] private Vector3 _tabSelectionOffset;
        [SerializeField] private Vector2 _tabSelectionScale = Vector2.one;
        
        [Serializable]
        private struct Tab {
            public UiButton button;
            public UiWindow window;
        }

        private IUiNavigationService _navigationService;
        private IUiWindowService _windowService;

        private CancellationTokenSource _enableCts;
        
        private RectTransform _tabSelectionInstance;
        private int _openedTab = -1;
        private byte _selectOperationId;
        
        private void Awake() {
            _navigationService = Services.Get<IUiNavigationService>();
            _windowService = Services.Get<IUiWindowService>();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];
                tab.button.OnClicked += OnClicked;
            }

            _navigationService.OnSelectableChanged += OnSelectableChanged;
            
            OpenTab(0, selectButton: true);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];
                tab.button.OnClicked -= OnClicked;
            }

            _navigationService.OnSelectableChanged -= OnSelectableChanged;
            
            if (_openedTab >= 0) {
                ref var openedTab = ref _tabs[_openedTab];
                
                openedTab.window.Node?.SetCurrentSelectable(null);
                _navigationService.RemoveWindowNavigationCallback(openedTab.window, this);
                
                PrefabPool.Main?.Release(_tabSelectionInstance);
                _tabSelectionInstance = null;
                _openedTab = -1;
            }
        }

        private void OnSelectableChanged(Selectable selectable, IUiWindow window) {
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];
                if (tab.button.Selectable != selectable) continue;
                
                OpenTab(i, selectButton: false);
                return;
            }
        }

        public bool CanNavigateBack() {
            return true;
        }
        
        public void OnNavigateBack() {
            if (_openedTab > 0) {
                OpenTab(0, selectButton: true);
                return;
            }

            if (_windowService.FindClosestParentWindow(gameObject) is { } parentWindow) {
                _windowService.SetWindowState(parentWindow, UiWindowState.Closed);
            }
        }

        private void OnClicked(UiButton button) {
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];
                if (tab.button != button) continue;

                if (_openedTab == i) {
                    if (tab.window.Node?.DefaultSelectable != null) {
                        _navigationService.SetCurrentSelectable(tab.window.Node.DefaultSelectable);
                    }
                    return;
                }

                OpenTab(i, selectButton: false);
                return;
            }
        }

        private void OpenTab(int index, bool selectButton) {
            if (index == _openedTab) return;
            _openedTab = index;
            
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];

                tab.window.Node?.SetCurrentSelectable(null);
                
                if (index == i) {
                    if (_setupMinState.HasValue) tab.button.GetComponent<IUiElementAnimator>()?.SetForceMinState(_setupMinState.Value);
                    _windowService.SetWindowState(tab.window, UiWindowState.Opened); 
                    _navigationService.AddWindowNavigationCallback(tab.window, this);
                    if (selectButton) _navigationService.SetCurrentSelectable(tab.button.Selectable);
                    continue;
                }

                tab.button.GetComponent<IUiElementAnimator>()?.ResetForceMinState();
                
                _navigationService.RemoveWindowNavigationCallback(tab.window, this);
            }
            
            SetupTabSelection(index, _enableCts.Token).Forget();
        }

        private async UniTask SetupTabSelection(int index, CancellationToken cancellationToken) {
            byte id = _selectOperationId.IncrementUncheckedRef();
            
            if (_tabSelectionPrefab != null && _tabSelectionInstance == null) {
                _tabSelectionInstance = PrefabPool.Main.Get(_tabSelectionPrefab, _tabSelectionParent);
            }

            _tabSelectionInstance.localScale = _tabSelectionScale.WithZ(1f);
            
            for (int i = 0; i < _tabs.Length; i++) {
                if (index != i) _tabs[i].button.GetComponent<IUiElementAnimator>()?.ResetForceMinState();
            }

            var tab = _tabs[index];
            
            if (_setupMinState.HasValue) tab.button.GetComponent<IUiElementAnimator>()?.SetForceMinState(_setupMinState.Value);
            
            var buttonRect = tab.button.GetComponent<RectTransform>();
            _tabSelectionInstance.anchoredPosition3D = buttonRect.anchoredPosition3D + _tabSelectionOffset;
            
            await UniTask.Yield();
            if (id != _selectOperationId || cancellationToken.IsCancellationRequested) return;
            
            // Set pos again to wait layout
            _tabSelectionInstance.anchoredPosition3D = buttonRect.anchoredPosition3D + _tabSelectionOffset;
        }

        private void OnValidate() {
            if (!Application.isPlaying || _tabSelectionInstance == null) return;
            
            _tabSelectionInstance.localScale = _tabSelectionScale.WithZ(1f);
            
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];

                if (_openedTab == i) {
                    var pos = tab.button.GetComponent<RectTransform>().anchoredPosition3D;
                    _tabSelectionInstance.anchoredPosition3D = pos + _tabSelectionOffset;
                    break;   
                }
            }
        }
    }
    
}