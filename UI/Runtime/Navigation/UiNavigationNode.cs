using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Navigation {
    
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class UiNavigationNode : MonoBehaviour, IUiNavigationNode {
        
        [Header("Inner Navigation")]
        [SerializeField] private UiNavigationMode _mode;
        [SerializeField] private Vector2 _cell = new(1000f, 50f);
        [SerializeField] private UiNavigationLoop _loop = UiNavigationLoop.Vertical;
        [SerializeField] private Selectable _firstSelected;
        [SerializeField] private bool _scrollable = false;
        [VisibleIf(nameof(_scrollable))]
        [SerializeField] private RectTransform _viewport;
        
        [Header("Outer Navigation")]
        [SerializeField] private UiIncomingOuterNavigationOptions _incomingNavigation = UiIncomingOuterNavigationOptions.SelectHistoryElement;
        [SerializeField] private UiNavigationMask _outcomingNavigation = ~UiNavigationMask.None;
        
        public GameObject GameObject => gameObject;
        public Selectable CurrentSelectable { get; private set; }
        public Selectable DefaultSelectable => _firstSelected;
        public UiIncomingOuterNavigationOptions IncomingOuterNavigation => _incomingNavigation;
        public bool IsScrollable => _scrollable;
        public RectTransform Viewport => _viewport;

        private IUiNavigationService _navigationService;
        
        private readonly UiNavigationNodeHelper _helper = new();
        private CancellationTokenSource _enableCts;
        private IUiWindow _window;
        private bool _hasSetupCurrentSelectable;
        
        private void Awake() {
            if (!_hasSetupCurrentSelectable) CurrentSelectable = _firstSelected;
            
            _navigationService = Services.Get<IUiNavigationService>();
            _window = GetComponent<IUiWindow>();
        }

        private void OnDestroy() {
            _helper.Dispose();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);

            if (_window != null) {
                _window.OnBeforeStateChanged += OnBeforeWindowStateChanged;
                _window.OnAfterStateChanged += OnAfterWindowStateChanged;
            }
            
            _navigationService.OnSelectableChanged += OnSelectedGameObjectChanged;
                
            if (_window == null || _window.State == UiWindowState.Opened) {
                _navigationService.BindNavigation(this);
            }
            
            UpdateNavigation();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            if (_window != null) {
                _window.OnBeforeStateChanged -= OnBeforeWindowStateChanged;
                _window.OnAfterStateChanged -= OnAfterWindowStateChanged;
            }
            
            _navigationService.OnSelectableChanged -= OnSelectedGameObjectChanged;
            
            _navigationService.UnbindNavigation(this);
        }

        private void OnBeforeWindowStateChanged(UiWindowState state) {
            if (state != UiWindowState.Opened) return;
            
            _navigationService.BindNavigation(this);
        }
        
        private void OnAfterWindowStateChanged(UiWindowState state) {
            if (state != UiWindowState.Closed) return;
            
            _navigationService.UnbindNavigation(this);
        }
        
        public void BindSelectable(Selectable selectable, UiNavigationMask mask = ~UiNavigationMask.None, UiNavigationOptions options = UiNavigationOptions.None) {
            _helper.Bind(selectable, mask, options);
            OnSelectedGameObjectChanged(_navigationService.CurrentSelectable, _navigationService.SelectedGameObjectWindow);   
        }

        public void UnbindSelectable(Selectable selectable) {
            _helper.Unbind(selectable);
        }

#if UNITY_EDITOR
        [Button(mode: ButtonAttribute.Mode.Runtime)]
#endif
        public void UpdateNavigation() {
            _helper.UpdateNavigationNextFrame(transform, _mode, _loop, _cell, _enableCts?.Token ?? CancellationToken.None).Forget();
        }

        public void OnNavigateOut(Selectable fromSelectable, UiNavigationDirection direction) {
            _helper.NavigateOut(this, fromSelectable, direction, _cell, _outcomingNavigation, _loop);
        }

        public void SetCurrentSelectable(Selectable selectable) {
            _hasSetupCurrentSelectable = true;
            CurrentSelectable = selectable;
        }

        private void OnSelectedGameObjectChanged(Selectable selected, IUiWindow parent) {
            if (selected != null && _helper.IsBound(selected.gameObject)) {
                CurrentSelectable = selected;
            }
        }
    }
    
}