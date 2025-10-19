using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Service;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    [DisallowMultipleComponent]
    public sealed class UiNavigationNode : MonoBehaviour, IUiNavigationNode {
        
        [Header("Inner Navigation")]
        [SerializeField] private UiNavigationMode _mode;
        [SerializeField] private Vector2 _cell = new(400f, 50f);
        [SerializeField] private bool _loop = true;
        [SerializeField] private bool _scrollable = false;
        [VisibleIf(nameof(_scrollable))]
        [SerializeField] private RectTransform _viewport;
        
        [Header("Outer Navigation")]
        [SerializeField] private UiNavigateFromOuterNodesOptions _navigateFromOuterNodesOptions = 
                UiNavigateFromOuterNodesOptions.SelectClosestElement;
        
        [SerializeField] private UiNavigateToOuterNodesOptions _navigateToOuterNodesOptions = 
            UiNavigateToOuterNodesOptions.Parent | UiNavigateToOuterNodesOptions.Siblings | UiNavigateToOuterNodesOptions.Children;
        
        public GameObject GameObject => gameObject;
        public GameObject CurrentSelected { get; private set; }
        public UiNavigateFromOuterNodesOptions NavigateFromOuterNodesOptions => _navigateFromOuterNodesOptions;
        public bool IsScrollable => _scrollable;
        public RectTransform Viewport => _viewport;
        
        private readonly UiNavigationNodeHelper _helper = new();
        private CancellationTokenSource _enableCts;
        
        private IUiWindow _window;
        
        private void Awake() {
            _window = GetComponent<IUiWindow>();
        }

        private void OnDestroy() {
            _helper.Dispose();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;
            }

            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.OnSelectedGameObjectChanged += OnSelectedGameObjectChanged;
                
                if (_window == null || _window.State == UiWindowState.Opened) {
                    navigationService.BindNavigation(this);
                }
            }
            
            UpdateNavigation();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged -= OnWindowsHierarchyChanged;
            }

            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.OnSelectedGameObjectChanged -= OnSelectedGameObjectChanged;
                
                navigationService.UnbindNavigation(this);
            }
        }

        public void Bind(Selectable selectable, UiNavigationMask mask = ~UiNavigationMask.None) {
            _helper.Bind(selectable, mask);
            
            if (Services.TryGet(out IUiNavigationService service)) {
                OnSelectedGameObjectChanged(service.SelectedGameObject, service.SelectedGameObjectWindow);   
            }
        }

        public void Unbind(Selectable selectable) {
            _helper.Unbind(selectable);
        }

#if UNITY_EDITOR
        [Button(mode: ButtonAttribute.Mode.Runtime)]
#endif
        public void UpdateNavigation() {
            _helper.UpdateNavigationNextFrame(transform, _mode, _loop, _cell, _enableCts?.Token ?? default).Forget();
        }

        public void OnNavigateOut(Selectable fromSelectable, UiNavigationDirection direction) {
            _helper.NavigateOut(this, fromSelectable, direction, _navigateToOuterNodesOptions);
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