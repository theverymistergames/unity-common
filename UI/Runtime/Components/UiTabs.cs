using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
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
        
        [Header("Tabs")]
        [SerializeField] private Transform _tabSelectionParent;
        [SerializeField] private Tab[] _tabs;
        
        [Header("Focus")]
        [SerializeField] private FocusMode _focusMode = FocusMode.TabsOrTabContentWithRelativeButton;
        [VisibleIf(nameof(_focusMode), 1)]
        [SerializeField] [Range(0f, 1f)] private float _alpha = 0.25f;
        
        [Header("Tab Selection")]
        [SerializeField] private Optional<UiElementState> _setupMinState = new(UiElementState.Hover, hasValue: false);
        [SerializeField] private RectTransform _tabSelectionPrefab;
        [SerializeField] private Vector3 _tabSelectionOffset;
        [SerializeField] private Vector2 _tabSelectionScale = Vector2.one;
        
        [Serializable]
        private struct Tab {
            public UiButton button;
            public UiWindow window;
        }

        private enum FocusMode {
            None,
            TabsOrTabContent,
            TabsOrTabContentWithRelativeButton,    
        }
        
        private IUiNavigationService _navigationService;
        private IUiWindowService _windowService;

        private CancellationTokenSource _enableCts;
        private CanvasGroup[] _canvasGroupTabButtons;
        private CanvasGroup[] _canvasGroupTabContents;
        private RectTransform _tabSelectionInstance;
        private CanvasGroup _canvasGroupTabSelection;
        private IUiElementAnimator _tabSelectionAnimator;
        private IUiElementAnimator _tabButtonAnimator;
        private int _openedTab = -1;
        private byte _selectOperationId;
        
        private void Awake() {
            _navigationService = Services.Get<IUiNavigationService>();
            _windowService = Services.Get<IUiWindowService>();
            
            FetchCanvasGroups();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];
                tab.button.OnClicked += OnClicked;
            }

            _navigationService.OnSelectableChanged += OnSelectableChanged;
            
            OpenTab(0, selectButton: true);
            CheckFocus();
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

                BindTabButtonAnimator(null);
                _tabSelectionAnimator?.ResetMinState();
                _tabSelectionAnimator = null;

                PrefabPool.Main?.Release(_tabSelectionInstance);
                _tabSelectionInstance = null;
                _canvasGroupTabSelection = null;
                _openedTab = -1;
            }
        }

        private void FetchCanvasGroups() {
            int length = _tabs?.Length ?? 0;
            
            if (_canvasGroupTabButtons == null || _canvasGroupTabButtons.Length != length) {
                _canvasGroupTabButtons = new CanvasGroup[length];
            }
            
            if (_canvasGroupTabContents == null || _canvasGroupTabContents.Length != length) {
                _canvasGroupTabContents = new CanvasGroup[length];
            }
            
            for (int i = 0; i < _tabs?.Length; i++) {
                ref var tab = ref _tabs[i];
                
                _canvasGroupTabButtons[i] = tab.button.gameObject.GetOrAddComponent<CanvasGroup>();
                _canvasGroupTabContents[i] = tab.window.gameObject.GetOrAddComponent<CanvasGroup>();
            }
        }
        
        private void OnSelectableChanged(Selectable selectable, IUiWindow window) {
            CheckOpenedTab(selectable);
            CheckFocus();
        }

        private void CheckOpenedTab(Selectable selectable) {
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];
                if (tab.button.Selectable != selectable) continue;
                
                OpenTab(i, selectButton: false);
                return;
            }
        }

        private void CheckFocus() {
            if (_focusMode == FocusMode.None) {
                for (int i = 0; i < _tabs?.Length; i++) {
                    SetFocusedState(_canvasGroupTabButtons[i], true);
                    SetFocusedState(_canvasGroupTabContents[i], true);
                }

                SetFocusedState(_canvasGroupTabSelection, true);
                return;
            }
            
            var currentSelected = _navigationService.CurrentSelectable;
            int focusedButtonIndex = -1;
            
            for (int i = 0; i < _tabs?.Length; i++) {
                ref var tab = ref _tabs[i];
                if (currentSelected != tab.button.Selectable) continue;

                focusedButtonIndex = i;
                break;
            }
            
            for (int i = 0; i < _tabs?.Length; i++) {
                ref var tab = ref _tabs[i];

                bool focusedTabButton = focusedButtonIndex >= 0 || 
                                        _openedTab == i && _focusMode == FocusMode.TabsOrTabContentWithRelativeButton;
                
                bool focusedTabContent = focusedButtonIndex < 0 &&
                                         ReferenceEquals(_navigationService.GetParentWindow(currentSelected), tab.window);
                
                SetFocusedState(_canvasGroupTabButtons[i], focusedTabButton);
                SetFocusedState(_canvasGroupTabContents[i], focusedTabContent);

                if (_openedTab == i) SetFocusedState(_canvasGroupTabSelection, focusedTabButton);
            }
        }

        private void SetFocusedState(CanvasGroup canvasGroup, bool focused) {
            if (canvasGroup == null) return;
            
            canvasGroup.alpha = focused ? 1f : _alpha;
        }
        
        public bool CanNavigateBack() {
            if (_openedTab < 0) return true;
            
            ref var tab = ref _tabs[_openedTab];

            if (_navigationService.CurrentSelectable != null 
                && (tab.window?.Node?.IsBoundSelectable(_navigationService.CurrentSelectable) ?? false)) 
            {
                _navigationService.SetCurrentSelectable(tab.button.Selectable);
                return false;
            }
            
            if (_openedTab > 0) {
                OpenTab(0, selectButton: true);   
                return false;
            }
            
            return true;
        }

        void IUiNavigationCallback.OnNavigateBack() {
            if (_windowService.FindClosestParentWindow(gameObject) is { } parentWindow) {
                _windowService.SetWindowState(parentWindow, UiWindowState.Closed);
            }
        }

        private void OnClicked(UiButton button) {
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];
                if (tab.button != button) continue;

                if (_openedTab == i) {
                    var node = tab.window.Node;
                    var target = node?.CurrentSelectable != null ? node.CurrentSelectable : node?.DefaultSelectable;

                    if (target != null) {
                        _navigationService.SetCurrentSelectable(target);
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
                    if (_setupMinState.HasValue) tab.button.GetComponent<IUiElementAnimator>()?.SetMinState(_setupMinState.Value);
                    _windowService.SetWindowState(tab.window, UiWindowState.Opened); 
                    _navigationService.AddWindowNavigationCallback(tab.window, this);
                    if (selectButton) _navigationService.SetCurrentSelectable(tab.button.Selectable);
                    continue;
                }

                tab.button.GetComponent<IUiElementAnimator>()?.ResetMinState();
                
                _navigationService.RemoveWindowNavigationCallback(tab.window, this);
            }
            
            SetupTabSelection(index, _enableCts.Token).Forget();
        }

        private async UniTask SetupTabSelection(int index, CancellationToken cancellationToken) {
            byte id = _selectOperationId.IncrementUncheckedRef();
            
            if (_tabSelectionPrefab != null && _tabSelectionInstance == null) {
                _tabSelectionInstance = PrefabPool.Main.Get(_tabSelectionPrefab, _tabSelectionParent, worldPositionStays: false);
                _tabSelectionAnimator = _tabSelectionInstance.GetComponent<IUiElementAnimator>();
                _canvasGroupTabSelection = _tabSelectionInstance.gameObject.GetOrAddComponent<CanvasGroup>();
            }

            _tabSelectionInstance.localScale = _tabSelectionScale.WithZ(1f);
            
            for (int i = 0; i < _tabs.Length; i++) {
                if (index != i) _tabs[i].button.GetComponent<IUiElementAnimator>()?.ResetMinState();
            }

            var tab = _tabs[index];
            
            if (_setupMinState.HasValue) tab.button.GetComponent<IUiElementAnimator>()?.SetMinState(_setupMinState.Value);

            BindTabButtonAnimator(tab.button.GetComponent<IUiElementAnimator>());

            var buttonRect = tab.button.GetComponent<RectTransform>();
            _tabSelectionInstance.anchoredPosition3D = buttonRect.anchoredPosition3D + _tabSelectionOffset;
            
            await UniTask.Yield();
            if (id != _selectOperationId || cancellationToken.IsCancellationRequested) return;
            
            // Set pos again to wait layout
            _tabSelectionInstance.anchoredPosition3D = buttonRect.anchoredPosition3D + _tabSelectionOffset;
        }

        private void BindTabButtonAnimator(IUiElementAnimator buttonAnimator) {
            if (_tabButtonAnimator != null) _tabButtonAnimator.OnStateChanged -= OnTabButtonStateChanged;

            _tabButtonAnimator = buttonAnimator;

            if (_tabButtonAnimator == null) return;

            _tabButtonAnimator.OnStateChanged += OnTabButtonStateChanged;
            OnTabButtonStateChanged(_tabButtonAnimator.CurrentState);
        }

        private void OnTabButtonStateChanged(UiElementState state) {
            _tabSelectionAnimator?.SetMinState(state);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            ValidateTabSelection();
            
            if (Application.isPlaying && _navigationService != null) {
                FetchCanvasGroups();
                CheckFocus();
            }
        }

        private void ValidateTabSelection() {
            if (!Application.isPlaying || _tabSelectionInstance == null) return;
            
            _tabSelectionInstance.localScale = _tabSelectionScale.WithZ(1f);
            
            for (int i = 0; i < _tabs.Length; i++) {
                ref var tab = ref _tabs[i];

                if (_openedTab != i) continue;
                
                var pos = tab.button.GetComponent<RectTransform>().anchoredPosition3D;
                _tabSelectionInstance.anchoredPosition3D = pos + _tabSelectionOffset;
                break;
            }
        }
#endif
    }
    
}