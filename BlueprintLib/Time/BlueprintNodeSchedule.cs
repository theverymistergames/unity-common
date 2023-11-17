using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceSchedule :
        BlueprintSource<BlueprintNodeSchedule2>,
        BlueprintSources.IEnter<BlueprintNodeSchedule2>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Schedule", Category = "Time", Color = BlueprintColors.Node.Time)]
    public struct BlueprintNodeSchedule2 : IBlueprintNode, IBlueprintEnter2 {

        [SerializeField] [Min(0f)] private float _period;
        [SerializeField] [Min(1)] private int _times;
        [SerializeField] private bool _isInfinite;

        private CancellationTokenSource _terminateCts;
        private CancellationTokenSource _cancelCts;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Start"));
            meta.AddPort(id, Port.Enter("Cancel"));
            meta.AddPort(id, Port.Input<float>("Period"));
            meta.AddPort(id, Port.Input<int>("Times"));
            meta.AddPort(id, Port.Exit("On Period"));
            meta.AddPort(id, Port.Exit("On Finish"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _terminateCts = new CancellationTokenSource();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _terminateCts.Cancel();
            _terminateCts.Dispose();
            _terminateCts = null;

            _cancelCts?.Cancel();
            _cancelCts?.Dispose();
            _cancelCts = null;
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port == 0) {
                _cancelCts?.Cancel();
                _cancelCts?.Dispose();
                _cancelCts ??= new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancelCts.Token, _terminateCts.Token);

                float period = blueprint.Read(token, 2, _period);
                int times = blueprint.Read(token, 3, _times);

                ScheduleAsync(blueprint, token, period, times, _isInfinite, linkedCts.Token).Forget();
                return;
            }

            if (port == 1) {
                _cancelCts?.Cancel();
                _cancelCts?.Dispose();
                _cancelCts = null;
            }
        }

        private async UniTaskVoid ScheduleAsync(
            IBlueprint blueprint,
            NodeToken token,
            float period,
            int times,
            bool isInfinite,
            CancellationToken cancellationToken
        ) {
            int timesCounter = 0;

            while (!cancellationToken.IsCancellationRequested) {
                if (!isInfinite && timesCounter >= times) break;

                bool isCancelled = await UniTask
                    .Delay(TimeSpan.FromSeconds(period), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();

                if (isCancelled) return;

                timesCounter++;
                blueprint.Call(token, 4);
            }

            if (cancellationToken.IsCancellationRequested) return;

            blueprint.Call(token, 5);
        }
    }

    [Serializable]
    [BlueprintNodeMeta(Name = "Schedule", Category = "Time", Color = BlueprintColors.Node.Time)]
    public sealed class BlueprintNodeSchedule : BlueprintNode, IBlueprintEnter {

        [SerializeField] [Min(0f)] private float _period;
        [SerializeField] [Min(1)] private int _times;
        [SerializeField] private bool _isInfinite;

        private CancellationTokenSource _terminateCts;
        private CancellationTokenSource _cancelCts;
        
        public override Port[] CreatePorts() => new[] {
            Port.Enter("Start"),
            Port.Enter("Cancel"),
            Port.Input<float>("Period"),
            Port.Input<int>("Times"),
            Port.Exit("On Period"),
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
