using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceDelay :
        BlueprintSource<BlueprintNodeDelay>,
        BlueprintSources.IEnter<BlueprintNodeDelay>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Delay", Category = "Time", Color = BlueprintColors.Node.Time)]
    public struct BlueprintNodeDelay : IBlueprintNode, IBlueprintEnter {

        [SerializeField] private float _duration;

        private CancellationTokenSource _terminateCts;
        private CancellationTokenSource _cancelCts;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Start"));
            meta.AddPort(id, Port.Enter("Cancel"));
            meta.AddPort(id, Port.Input<float>("Duration"));
            meta.AddPort(id, Port.Exit("On Finish"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _terminateCts = new CancellationTokenSource();
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
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
                _cancelCts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancelCts.Token, _terminateCts.Token);

                float duration = blueprint.Read(token, 2, _duration);
                DelayAndExitAsync(blueprint, token, duration, linkedCts.Token).Forget();

                return;
            }

            if (port == 1) {
                _cancelCts?.Cancel();
                _cancelCts?.Dispose();
                _cancelCts = null;
            }
        }

        private async UniTaskVoid DelayAndExitAsync(IBlueprint blueprint, NodeToken token, float delay, CancellationToken cancellationToken) {
            bool isCancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();

            if (isCancelled) return;

            blueprint.Call(token, 3);
        }
    }

}
