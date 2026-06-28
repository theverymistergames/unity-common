using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.UI.Components {
    
    [RequireComponent(typeof(UiButton))]
    public sealed class UiButtonClickAction : MonoBehaviour, IActorComponent {
        
        [SerializeField] private UiButton _uiButton;
        [SerializeField] private ActionMode _actionMode = ActionMode.WaitPreviousAction;
        [SerializeField] private CancelMode _cancelMode = CancelMode.NonCancelable;
        [SerializeReference] [SubclassSelector] private IActorAction _clickAction;

        private enum ActionMode {
            InvokeNewAction,
            CancelPreviousAction,
            WaitPreviousAction,
        } 
        
        private enum CancelMode {
            NonCancelable,
            OnButtonDisabled,
            OnButtonDestroyed,
        }

        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _clickCts;
        private IActor _actor;
        private byte _clickActionId;
        private byte _awaitClickActionId;
        
        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _uiButton.OnClicked += OnClick;
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            AsyncExt.DisposeCts(ref _clickCts);
            
            _uiButton.OnClicked -= OnClick;
        }

        private void OnClick() {
            var cancellationToken = _cancelMode switch {
                CancelMode.NonCancelable => CancellationToken.None,
                CancelMode.OnButtonDisabled => _enableCts.Token,
                CancelMode.OnButtonDestroyed => _destroyCts.Token,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            switch (_actionMode) {
                case ActionMode.InvokeNewAction:
                    break;
                
                case ActionMode.CancelPreviousAction:
                    AsyncExt.RecreateCts(ref _clickCts);
                    cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _clickCts.Token).Token;
                    break;
                
                case ActionMode.WaitPreviousAction:
                    if (_clickActionId > _awaitClickActionId) return;

                    _clickActionId.IncrementUncheckedRef();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            WaitClickAction(cancellationToken).Forget();
        }

        private async UniTask WaitClickAction(CancellationToken cancellationToken) {
            if (_clickAction != null) await _clickAction.Apply(_actor, cancellationToken);
            _awaitClickActionId = _clickActionId.IncrementUncheckedRef();
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _uiButton = GetComponent<UiButton>();
        }
#endif
    }
    
}