using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Track Distance Transform", Category = "Transform", Color = BlueprintColors.Node.Data)]
    public class BlueprintNodeTrackDistanceTransform : IBlueprintNode, IBlueprintOutput<float>, IBlueprintEnter {

        private CancellationTokenSource _terminateCts;
        private CancellationTokenSource _cancelCts;

        private float _lastDistance;
        private float _lastSqrDistance;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Start"));
            meta.AddPort(id, Port.Enter("Stop"));
            meta.AddPort(id, Port.Input<Transform>("Transform A"));
            meta.AddPort(id, Port.Input<Transform>("Transform B"));
            meta.AddPort(id, Port.Exit("On Change"));
            meta.AddPort(id, Port.Output<float>("Distance"));
            meta.AddPort(id, Port.Output<float>("Sqr Distance"));
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

                var t0 = blueprint.Read<Transform>(token, port: 2);
                var t1 = blueprint.Read<Transform>(token, port: 3);

                TrackDistance(blueprint, token, t0, t1, linkedCts.Token).Forget();
                return;
            }

            if (port == 1) {
                _cancelCts?.Cancel();
                _cancelCts?.Dispose();
                _cancelCts = null;
                return;
            }
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            5 => _lastDistance,
            6 => _lastSqrDistance,
            _ => default,
        };

        private async UniTask TrackDistance(
            IBlueprint blueprint,
            NodeToken token,
            Transform t0,
            Transform t1,
            CancellationToken cancellationToken
        ) {
            while (!cancellationToken.IsCancellationRequested) {
                var position0 = t0.position;
                var position1 = t1.position;

                float sqrDistance = (position0 - position1).sqrMagnitude;

                // Notify change
                if (!_lastSqrDistance.IsNearlyEqual(sqrDistance)) {
                    _lastSqrDistance = sqrDistance;
                    _lastDistance = Vector3.Distance(position0, position1);

                    blueprint.Call(token, port: 4);
                }

                await UniTask.Yield();
            }
        }
    }

}
