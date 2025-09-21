using System;
using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    [DisallowMultipleComponent]
    public sealed class UiNavigationNode : MonoBehaviour, IUiNavigationNode {

        [SerializeField] private UiNavigationMode _mode;
        [SerializeField] private bool _loop = true;

        public GameObject GameObject => gameObject;
        public GameObject CurrentSelected { get; private set; }
        
        private readonly UiNavigationNodeHelper _helper = new();
        
        private IUiWindow _window;
        
        private void Awake() {
            _window = GetComponent<IUiWindow>();
        }

        private void OnDestroy() {
            _helper.Dispose();
        }

        private void OnEnable() {
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;
            }

            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.OnSelectedGameObjectChanged += OnSelectedGameObjectChanged;
                
                if (_window == null || _window.State == UiWindowState.Opened) {
                    navigationService.BindNavigation(this);
                }
            }
        }

        private void OnDisable() {
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged -= OnWindowsHierarchyChanged;
            }

            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.OnSelectedGameObjectChanged -= OnSelectedGameObjectChanged;
                
                navigationService.UnbindNavigation(this);
            }
        }
        
        public void Bind(Selectable selectable) {
            _helper.Bind(selectable);

            if (Services.TryGet(out IUiNavigationService service)) {
                OnSelectedGameObjectChanged(service.SelectedGameObject, service.SelectedGameObjectWindow);   
            }
        }

        public void Unbind(Selectable selectable) {
            _helper.Unbind(selectable);
        }

        public void UpdateNavigation() {
            _helper.UpdateNavigation(transform, _mode, _loop);
        }

        private void OnWindowsHierarchyChanged() {
            if (_window == null || !Services.TryGet(out IUiNavigationService service)) return;

            switch (_window.State) {
                case UiWindowState.Closed:
                    service.UnbindNavigation(this);
                    break;
                
                case UiWindowState.Opened:
                    service.BindNavigation(this);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnSelectedGameObjectChanged(GameObject selected, IUiWindow parent) {
            if (selected != null && _helper.IsBound(selected)) {
                CurrentSelected = selected;
            }
        }
    }
    
}