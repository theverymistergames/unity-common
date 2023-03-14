﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Schedule", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeSchedule : BlueprintNode, IBlueprintEnter {

        [SerializeField] [Min(0f)] private float _period;
        [SerializeField] [Min(1)] private int _times;
        [SerializeField] private bool _isInfinite;

        private CancellationTokenSource _terminateCts;
        private CancellationTokenSource _cancelCts;
        
        public override Port[] CreatePorts() => new[] {
            Port.Action(PortDirection.Input, "Start"),
            Port.Action(PortDirection.Input, "Cancel"),
            Port.Func<float>(PortDirection.Input, "Period"),
            Port.Func<int>(PortDirection.Input, "Times"),
            Port.Action(PortDirection.Output, "On Period"),
            Port.Action(PortDirection.Output, "On Finish"),
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
                _cancelCts?.Cancel();
                _cancelCts?.Dispose();
                _cancelCts ??= new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancelCts.Token, _terminateCts.Token);

                float period = Ports[2].Get(_period);
                int times = Ports[3].Get(_times);

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
                if (!isInfinite && timesCounter >= times) break;

                bool isCancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(period), cancellationToken: token)
                    .SuppressCancellationThrow();

                if (isCancelled) return;

                timesCounter++;
                Ports[4].Call();
            }

            if (token.IsCancellationRequested) return;

            Ports[5].Call();
        }
    }

}
