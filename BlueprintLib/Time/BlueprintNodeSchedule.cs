using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Schedule", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeSchedule : BlueprintNode, IBlueprintEnter {

        [SerializeField] [Min(0f)] private float _period;
        [SerializeField] [Min(1)] private int _times;
        [SerializeField] private bool _isInfinite;

        private CancellationTokenSource _terminateCts;
        private CancellationTokenSource _cancelCts;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Period"),
            Port.Input<int>("Times"),
            Port.Exit(),
        };

        protected override void OnInit() {
            _terminateCts = new CancellationTokenSource();
        }

        protected override void OnTerminate() {
            _terminateCts.Cancel();
            _terminateCts.Dispose();

            _cancelCts?.Cancel();
            _cancelCts?.Dispose();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) {
                _cancelCts ??= new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancelCts.Token, _terminateCts.Token);

                float period = Read(2, _period);
                int times = Read(3, _times);

                ScheduleAsync(period, times, _isInfinite, linkedCts.Token).Forget();
                return;
            }

            if (port == 1) {
                _cancelCts?.Cancel();
                _cancelCts?.Dispose();
                _cancelCts = null;
            }
        }

        private async UniTaskVoid ScheduleAsync(float period, int times, bool isInfinite, CancellationToken token) {
            int timesCounter = 0;
            while (!token.IsCancellationRequested) {
                if (!isInfinite && timesCounter >= times) return;

                bool isCancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(period), cancellationToken: token)
                    .SuppressCancellationThrow();

                if (isCancelled) return;

                timesCounter++;
                Call(port: 5);
            }
        }
    }

}
