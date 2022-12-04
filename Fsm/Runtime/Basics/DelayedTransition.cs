using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Fsm.Core;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Fsm.Basics {

    public sealed class DelayedTransition : FsmTransition {
        
        [SerializeField] private float _delay;

        private CancellationTokenSource _delayCts;
        private ITimeSource _timeSource;

        protected override void OnAttach(StateMachineRunner runner) {
            _timeSource = runner.TimeSource;

            _delayCts?.Dispose();
            _delayCts = new CancellationTokenSource();
        }

        protected override void OnDetach() {
            _delayCts.Cancel();
        }

        protected override void OnEnterSourceState() {
            StartDelay(_delayCts.Token).Forget();
        }

        protected override void OnExitSourceState() { }

        private async UniTaskVoid StartDelay(CancellationToken token) {
            bool isCanceled = await UniTask
                .Delay(TimeSpan.FromSeconds(_delay), cancellationToken: token)
                .SuppressCancellationThrow();

            if (isCanceled) return;

            Transit();
        }
    }

}
