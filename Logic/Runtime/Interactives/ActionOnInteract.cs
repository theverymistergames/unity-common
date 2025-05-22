using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Logic.Interactives {

    [RequireComponent(typeof(Interactive))]
    public sealed class ActionOnInteract : MonoBehaviour {
        
        [SerializeField] private CancelMode _cancelOnNextAction;
        [SerializeReference] [SubclassSelector] private IActorAction _startAction;
        [SerializeReference] [SubclassSelector] private IActorAction _stopAction;

        private enum CancelMode {
            DontCancel,
            CancelOnStartAndStop,
            CancelOnlyOnStart,
            CancelOnlyOnStop,
        }
        
        private Interactive _interactive;
        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _actionCts;
        
        private void Awake() {
            _interactive = GetComponent<Interactive>();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _interactive.OnStartInteract += OnStartInteract;
            _interactive.OnStopInteract += OnStopInteract;
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            AsyncExt.DisposeCts(ref _actionCts);
            
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;
        }

        private void OnStartInteract(IInteractiveUser user) {
            var token = _enableCts.Token;
            
            if (NeedCancelPrevious(isStartAction: true)) {
                AsyncExt.RecreateCts(ref _actionCts);
                token = CancellationTokenSource.CreateLinkedTokenSource(token, _actionCts.Token).Token;
            }

            if (user.Root.GetComponent<IActor>() is { } actor) _startAction?.Apply(actor, token).Forget();
        }

        private void OnStopInteract(IInteractiveUser user) {
            var token = _enableCts.Token;
            
            if (NeedCancelPrevious(isStartAction: false)) {
                AsyncExt.RecreateCts(ref _actionCts);
                token = CancellationTokenSource.CreateLinkedTokenSource(token, _actionCts.Token).Token;
            }
            
            if (user.Root.GetComponent<IActor>() is {} actor) _stopAction?.Apply(actor, token).Forget();
        }

        private bool NeedCancelPrevious(bool isStartAction) {
            return _cancelOnNextAction switch {
                CancelMode.DontCancel => false,
                CancelMode.CancelOnStartAndStop => true,
                CancelMode.CancelOnlyOnStart => isStartAction,
                CancelMode.CancelOnlyOnStop => !isStartAction,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

}
