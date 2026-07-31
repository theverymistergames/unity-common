using System;
using MisterGames.Common.Data;
using MisterGames.Common.Maths;
using MisterGames.Common.Pooling;
using MisterGames.Common.Service;
using MisterGames.UI.Data;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;

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
        private int _openedTab = -1;
        private RectTransform _tabSelectionInstance;

        private void Awake() {
            _navigationService = Services.Get<IUiNavigationService>();
            _windowService = Services.Get<IUiWindowService>();
        }

        private void OnEnable() {
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];
                tab.button.OnClicked += OnClicked;
            }

            OpenTab(0);
        }

        private void OnDisable() {
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];
                tab.button.OnClicked -= OnClicked;
            }

            if (_openedTab >= 0) {
                ref var openedTab = ref _tabs[_openedTab];
                _navigationService.RemoveWindowNavigationCallback(openedTab.window, this);
                PrefabPool.Main?.Release(_tabSelectionInstance);
            }
        }

        public bool CanNavigateBack() {
            return true;
        }
        
        public void OnNavigateBack() {
            if (_openedTab > 0) {
                OpenTab(0);
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
                
                OpenTab(i);
                return;
            }
        }

        private void OpenTab(int index) {
            _openedTab = index;
            
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];

                if (index == i) {
                    if (_setupMinState.HasValue) tab.button.GetComponent<IUiElementAnimator>()?.SetForceMinState(_setupMinState.Value);
                    _windowService.SetWindowState(tab.window, UiWindowState.Opened); 
                    _navigationService.AddWindowNavigationCallback(tab.window, this);
                    continue;   
                }

                tab.button.GetComponent<IUiElementAnimator>()?.ResetForceMinState();
                _navigationService.RemoveWindowNavigationCallback(tab.window, this);
            }
            
            SetupTabSelection(index);
        }

        private void SetupTabSelection(int index) {
            if (_tabSelectionPrefab != null && _tabSelectionInstance == null) {
                _tabSelectionInstance = PrefabPool.Main.Get(_tabSelectionPrefab, _tabSelectionParent);
            }

            _tabSelectionInstance.localScale = _tabSelectionScale.WithZ(1f);
            
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];

                if (index == i) {
                    if (_setupMinState.HasValue) tab.button.GetComponent<IUiElementAnimator>()?.SetForceMinState(_setupMinState.Value);
                    var pos = tab.button.GetComponent<RectTransform>().anchoredPosition3D;
                    _tabSelectionInstance.anchoredPosition3D = pos + _tabSelectionOffset;
                    continue;   
                }

                tab.button.GetComponent<IUiElementAnimator>()?.ResetForceMinState();
            }
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