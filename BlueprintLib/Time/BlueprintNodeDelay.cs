using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Delay", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeDelay : BlueprintNode, IBlueprintEnter {
        
        [SerializeField] private float _defaultDuration;

        private CancellationTokenSource _terminateCts;
        private CancellationTokenSource _cancelCts;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Duration"),
            Port.Exit("On Finish"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _terminateCts = new CancellationTokenSource();
        }

        public override void OnDeInitialize() {
            _terminateCts.Cancel();
            _terminateCts.Dispose();

            _cancelCts?.Cancel();
            _cancelCts?.Dispose();
        }

        public void OnEnterPort(int port) {
            if (port == 0) {
                _cancelCts ??= new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancelCts.Token, _terminateCts.Token);

                float duration = ReadInputPort(2, _defaultDuration);
                DelayAndExitAsync(duration, linkedCts.Token).Forget();
                return;
            }

            if (port == 1) {
                _cancelCts?.Cancel();
                _cancelCts?.Dispose();
                _cancelCts = null;
            }
        }

        private async UniTaskVoid DelayAndExitAsync(float delay, CancellationToken token) {
            bool isCancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(delay), cancellationToken: token)
                .SuppressCancellationThrow();

            if (isCancelled) return;

            CallExitPort(3);
        }
    }

}
