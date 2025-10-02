using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.Labels;
using MisterGames.Common.Lists;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using MisterGames.Input.Core;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.UI.Components {
    
    public sealed class PauseMenuCallback : MonoBehaviour, IActorComponent {

        [Header("Input")]
        [SerializeField] private InputActionRef _openPauseMenuInput;
        [SerializeField] private InputMapRef[] _blockInputMaps;
        
        [Header("Window")]
        [SerializeField] [Min(0)] private int _openWindowOrder;
        [SerializeField] private UiWindow _menuWindow;

        [Header("Conditions")]
        [SubclassSelector]
        [SerializeReference] private IActorCondition _canOpenPauseMenu;

        [Header("Timescale")]
        [SerializeField] private LabelValue _timescalePriority;
        [SerializeField] [Min(0)] private int _changeTimescaleOrderOnOpen;
        [SerializeField] [Min(0)] private int _changeTimescaleOrderOnClose;
        [SerializeField] [Min(0f)] private float _timeScale = 1f;
        [SerializeField] [Min(0f)] private float _timeScaleDurationOnOpen;
        [SerializeField] [Min(0f)] private float _timeScaleDurationOnClose;
        [SerializeField] private AnimationCurve _timeScaleCurve = EasingType.Linear.ToAnimationCurve();
        
        [Header("Actions")]
        [SerializeField] private bool _cancelOnDisable = true;
        [SerializeField] private bool _cancelOnInputPressedBeforeWindowOpened = true;
        [SerializeField] private Action[] _actionsOnOpenMenu;
        [SerializeField] private Action[] _actionsOnCloseMenu;

        [Serializable]
        private struct Action {
            [Min(0)] public int order;
            [SubclassSelector]
            [SerializeReference] public IActorAction action;
        }
        
        private readonly List<int> _actionsIndicesBuffer = new();
        
        private CancellationTokenSource _actionCts;
        private IActor _actor;
        private float _startTime;
        private bool _isMenuOpened;
        private byte _openId;
        private byte _operationId;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnDestroy() {
            UnblockInputs();
            AsyncExt.DisposeCts(ref _actionCts);
        }

        private void OnEnable() {
            _startTime = TimeSources.scaledTime;
            
            if (Services.TryGet(out IUiWindowService service)) service.OnWindowsHierarchyChanged += OnWindowHierarchyChanged;
            
            _isMenuOpened = IsWindowRootInOpenedBranch(_menuWindow);

            _openPauseMenuInput.Get().performed += OnPauseInput;
            
            if (_isMenuOpened) {
                BlockInputs();
                Services.Get<ITimescaleSystem>().SetTimeScale(this, _timescalePriority.GetValue(), _timeScale);
            }
            else {
                UnblockInputs();
                Services.Get<ITimescaleSystem>().RemoveTimeScale(this);
            }
        }

        private void OnDisable() {
            if (_cancelOnDisable) {
                AsyncExt.DisposeCts(ref _actionCts);
                Services.Get<ITimescaleSystem>()?.RemoveTimeScale(this);
                UnblockInputs();
            }
            
            _openPauseMenuInput.Get().performed -= OnPauseInput;

            if (Services.TryGet(out IUiWindowService service)) service.OnWindowsHierarchyChanged -= OnWindowHierarchyChanged;
        }

        private void OnWindowHierarchyChanged() {
            bool wasMenuOpened = _isMenuOpened;
            _isMenuOpened = IsWindowRootInOpenedBranch(_menuWindow);

            if (!wasMenuOpened || _isMenuOpened) return;
            
            AsyncExt.RecreateCts(ref _actionCts);
                
            _openId = _operationId.IncrementUncheckedRef();
            _openId.IncrementUncheckedRef();
                
            OnCloseMenu(_actionCts.Token).Forget();
        }

        private void OnPauseInput(InputAction.CallbackContext obj) {
            if (_cancelOnInputPressedBeforeWindowOpened && _actionCts != null && _openId == _operationId) {
                AsyncExt.RecreateCts(ref _actionCts);
                
                _openId = _operationId.IncrementUncheckedRef();
                _openId.IncrementUncheckedRef();

                OnCloseMenu(_actionCts.Token).Forget();
                return;
            }

            if (!Services.TryGet(out IUiNavigationService service) || 
                service.IsExitToPauseBlocked() ||
                _menuWindow == null ||
                IsWindowRootInOpenedBranch(_menuWindow) ||
                !IsConditionMatch(_canOpenPauseMenu)) 
            {
                return;
            }
            
            AsyncExt.RecreateCts(ref _actionCts);
            _openId = _operationId.IncrementUncheckedRef();
            OpenMenu(_menuWindow, _actionCts.Token).Forget();
        }

        private async UniTask OpenMenu(IUiWindow window, CancellationToken cancellationToken) {
            BlockInputs(cancellationToken);
            
            var order = GetActionsOrder(onOpen: true, out int actionsCount);
            int timescalePriority = _timescalePriority.GetValue();
            
            var tasks = ArrayPool<UniTask>.Shared.Rent(actionsCount);
            
            for (int i = 0; i < order.Count && !cancellationToken.IsCancellationRequested; i++) {
                int index = order[i];

                int parallelCount = 0;
                
                if (index == _openWindowOrder) {
                    Services.Get<IUiWindowService>()?.SetWindowState(window, UiWindowState.Opened);
                }

                if (index == _changeTimescaleOrderOnOpen) {
                    tasks[parallelCount++] = Services.Get<ITimescaleSystem>().ChangeTimeScale(
                        source: this,
                        timescalePriority,
                        _timeScale,
                        _timeScaleDurationOnOpen,
                        removeOnFinish: false,
                        _timeScaleCurve,
                        cancellationToken
                    );
                }

                for (int j = 0; j < _actionsOnOpenMenu.Length; j++) {
                    var action = _actionsOnOpenMenu[j];
                    if (action.order != index || action.action == null) continue;
                    
                    tasks[parallelCount++] = action.action.Apply(_actor, cancellationToken);
                }

                if (parallelCount > 0) {
                    for (int j = parallelCount; j < actionsCount; j++) {
                        tasks[j] = UniTask.CompletedTask;
                    }
                    
                    await UniTask.WhenAll(tasks);
                }
            }
            
            tasks.ResetArrayElements();
            
            ArrayPool<UniTask>.Shared.Return(tasks);
        }
        
        private async UniTask OnCloseMenu(CancellationToken cancellationToken) {
            UnblockInputs();
            
            var order = GetActionsOrder(onOpen: false, out int actionsCount);
            int timescalePriority = _timescalePriority.GetValue();
            var tasks = ArrayPool<UniTask>.Shared.Rent(actionsCount);
            
            for (int i = 0; i < order.Count && !cancellationToken.IsCancellationRequested; i++) {
                int index = order[i];

                int parallelCount = 0;
                
                if (i == _changeTimescaleOrderOnClose) {
                    tasks[parallelCount++] = Services.Get<ITimescaleSystem>().ChangeTimeScale(
                        source: this,
                        timescalePriority,
                        1f,
                        _timeScaleDurationOnClose,
                        removeOnFinish: true,
                        _timeScaleCurve,
                        cancellationToken
                    );
                }

                for (int j = 0; j < _actionsOnCloseMenu.Length; j++) {
                    var action = _actionsOnCloseMenu[j];
                    if (action.order != index || action.action == null) continue;
                    
                    tasks[parallelCount++] = action.action.Apply(_actor, cancellationToken);
                }

                if (parallelCount > 0) {
                    for (int j = parallelCount; j < actionsCount; j++) {
                        tasks[j] = UniTask.CompletedTask;
                    }
                    
                    await UniTask.WhenAll(tasks);
                }
            }

            tasks.ResetArrayElements();
            
            ArrayPool<UniTask>.Shared.Return(tasks);
        }

        private void BlockInputs(CancellationToken cancellationToken = default) {
            InputServices.Blocks?.BlockInputMaps(this, _blockInputMaps, cancellationToken);
        }

        private void UnblockInputs() {
            InputServices.Blocks?.ClearAllInputMapBlocksOf(this);
        }

        private IReadOnlyList<int> GetActionsOrder(bool onOpen, out int actionsCount) {
            _actionsIndicesBuffer.Clear();

            var actions = onOpen ? _actionsOnOpenMenu : _actionsOnCloseMenu;
            actionsCount = 0;
            
            var set = new NativeHashSet<int>(2 + actions?.Length ?? 0, Allocator.Temp);

            if (onOpen) {
                set.Add(_openWindowOrder);
                set.Add(_changeTimescaleOrderOnOpen);
                actionsCount += 2;
            }
            else {
                set.Add(_changeTimescaleOrderOnClose);
                actionsCount += 1;
            }
            
            for (int i = 0; i < actions?.Length; i++) {
                ref var action = ref actions[i];
                if (action.action == null) continue;
                
                set.Add(actions[i].order);
                actionsCount++;
            }

            foreach (int i in set) {
                _actionsIndicesBuffer.Add(i);
            }
            
            _actionsIndicesBuffer.Sort();
            
            return _actionsIndicesBuffer;
        }
        
        private static bool IsWindowRootInOpenedBranch(IUiWindow window) {
            return window != null &&
                   Services.TryGet(out IUiWindowService windowService) &&
                   windowService.GetRootWindow(window) is { } rootWindow &&
                   windowService.IsInOpenedBranch(rootWindow);
        }

        private bool IsConditionMatch(IActorCondition condition) {
            return condition == null || condition.IsMatch(_actor, _startTime);
        }
    }
    
}