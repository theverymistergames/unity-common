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
        
        [SerializeField] [Min(0f)] private float _startDelay;
        [SerializeField] [Min(0f)] private float _period;
        [SerializeField] [Min(1)] private int _times;
        [SerializeField] private bool _isInfinite;

        private CancellationTokenSource _scheduleCts;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Start Delay"),
            Port.Input<float>("Period"),
            Port.Input<int>("Times"),
            Port.Exit(),
        };

        protected override void OnInit() {
            _scheduleCts?.Cancel();
            _scheduleCts?.Dispose();
        }

        protected override void OnTerminate() {
            _scheduleCts?.Cancel();
            _scheduleCts?.Dispose();
        }

        void IBlueprintEnter.Enter(int port) {
            if (port == 0) {
                _scheduleCts?.Cancel();
                _scheduleCts?.Dispose();
                _scheduleCts = new CancellationTokenSource();

                float startDelay = Read(2, _startDelay);
                float period = Read(3, _period);
                int times = Read(4, _times);
                
                StartSchedule(startDelay, period, times, _isInfinite, _scheduleCts.Token).Forget();
                return;
            }

            if (port == 1) {
                _scheduleCts?.Cancel();
                _scheduleCts?.Dispose();
            }
        }

        private async UniTaskVoid StartSchedule(float startDelay, float period, int times, bool isInfinite, CancellationToken token) {
            bool isCancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(startDelay), cancellationToken: token)
                .SuppressCancellationThrow();

            if (isCancelled) return;

            int counter = 0;
            while (!token.IsCancellationRequested) {
                if (!isInfinite && counter >= times) break;

                Call(port: 5);
                counter++;

                isCancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(period), cancellationToken: token)
                    .SuppressCancellationThrow();

                if (isCancelled) break;
            }
        }
    }

}
