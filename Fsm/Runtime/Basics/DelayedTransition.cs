using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Fsm.Core;
using UnityEngine;

namespace MisterGames.Fsm.Basics {

    public sealed class DelayedTransition : FsmTransition {
        
        [SerializeField] private float _delay;

        private CancellationTokenSource _cts;

        protected override void OnAttach(StateMachineRunner runner) { }

        protected override void OnDetach() {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        protected override void OnEnterSourceState() {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            DelayAndTransit(_delay, _cts.Token).Forget();
        }

        private async UniTaskVoid DelayAndTransit(float delay, CancellationToken token) {
            bool isCancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(delay), cancellationToken: token)
                .SuppressCancellationThrow();

            if (isCancelled) return;

            Transit();
        }

        protected override void OnExitSourceState() { }
    }

}
