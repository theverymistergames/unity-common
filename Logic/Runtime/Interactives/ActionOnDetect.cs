using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Interact.Detectables;
using UnityEngine;

namespace MisterGames.Logic.Interactives {

    public sealed class ActionOnDetect : MonoBehaviour {

        [SerializeField] private Detectable _detectable;
        [SerializeField] private CancelMode _cancelOnNextAction;
        [SerializeReference] [SubclassSelector] private IActorAction _detectAction;
        [SerializeReference] [SubclassSelector] private IActorAction _loseAction;

        private enum CancelMode {
            DontCancel,
            CancelOnDetectAndLose,
            CancelOnlyOnDetect,
            CancelOnlyOnLose,
        }

        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _actionCts;

        private void Awake() {
            if (_detectable == null) _detectable = GetComponent<Detectable>();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);

            _detectable.OnDetectedBy += OnDetectedBy;
            _detectable.OnLostBy += OnLostBy;
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            AsyncExt.DisposeCts(ref _actionCts);

            _detectable.OnDetectedBy -= OnDetectedBy;
            _detectable.OnLostBy -= OnLostBy;
        }

        private void OnDetectedBy(IDetector detector) {
            if (_enableCts == null) return;

            var token = _enableCts.Token;

            if (NeedCancelPrevious(isDetectAction: true)) {
                _actionCts?.Cancel();
                _actionCts?.Dispose();
                _actionCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                token = _actionCts.Token;
            }

            if (detector.Root.GetComponent<IActor>() is { } actor) _detectAction?.Apply(actor, token).Forget();
        }

        private void OnLostBy(IDetector detector) {
            if (_enableCts == null) return;

            var token = _enableCts.Token;

            if (NeedCancelPrevious(isDetectAction: false)) {
                _actionCts?.Cancel();
                _actionCts?.Dispose();
                _actionCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                token = _actionCts.Token;
            }

            if (detector.Root.GetComponent<IActor>() is { } actor) _loseAction?.Apply(actor, token).Forget();
        }

        private bool NeedCancelPrevious(bool isDetectAction) {
            return _cancelOnNextAction switch {
                CancelMode.DontCancel => false,
                CancelMode.CancelOnDetectAndLose => true,
                CancelMode.CancelOnlyOnDetect => isDetectAction,
                CancelMode.CancelOnlyOnLose => !isDetectAction,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

#if UNITY_EDITOR
        private void Reset() {
            _detectable = GetComponent<Detectable>();
        }
#endif
    }

}
