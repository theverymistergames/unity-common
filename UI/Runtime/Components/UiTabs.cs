using System;
using MisterGames.Common.Service;
using MisterGames.UI.Data;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;

namespace MisterGames.UI.Components {
    
    public sealed class UiTabs : MonoBehaviour, IUiNavigationCallback {
        
        [SerializeField] private Tab[] _tabs;
        
        [Serializable]
        private struct Tab {
            public UiButton button;
            public UiWindow window;
        }

        private IUiNavigationService _navigationService;
        private IUiWindowService _windowService;
        private int _openedTab = -1;

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
                    tab.button.GetComponent<IUiElementAnimator>()?.SetForceState(UiElementState.Selected);
                    _windowService.SetWindowState(tab.window, UiWindowState.Opened); 
                    _navigationService.AddWindowNavigationCallback(tab.window, this);
                    continue;   
                }

                tab.button.GetComponent<IUiElementAnimator>()?.ResetForceState();
                _navigationService.RemoveWindowNavigationCallback(tab.window, this);
            }
        }
    }
    
}