using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Common.Attributes;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Tween Tracker", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenTracker :
        IBlueprintNode,
        IBlueprintOutput<ITween>,
        IBlueprintOutput<float>,
        ITween
    {
        [SerializeField] private bool _invertLayout;
        [SerializeField] private bool _routeOnCancelledIntoOnFinished;

        private (ITween tween, float duration) _internalTween;

        private IBlueprint _blueprint;
        private NodeToken _token;

        private float _speed;
        private float _progress;
        private bool _isFirstNotifyProgress;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(
                id,
                Port.Output<ITween>("Self")
                    .Capacity(PortCapacity.Single)
                    .Layout(_invertLayout ? PortLayout.Right : PortLayout.Left)
            );

            meta.AddPort(
                id,
                Port.Input<ITween>("Tween")
                    .Layout(_invertLayout ? PortLayout.Left : PortLayout.Right)
            );

            meta.AddPort(id, Port.Exit("On Start"));
            meta.AddPort(id, Port.Exit("On Finished"));
            meta.AddPort(id, Port.Exit("On Cancelled"));
            meta.AddPort(id, Port.Exit("On Update"));
            meta.AddPort(id, Port.Output<float>("Progress"));
            meta.AddPort(id, Port.Output<float>("Speed"));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _token = token;
            _blueprint = blueprint;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = null;
        }

        ITween IBlueprintOutput<ITween>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;
            _blueprint = blueprint;

            return port == 0 ? this : default;
        }

        float IBlueprintOutput<float>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;
            _blueprint = blueprint;

            return port switch {
                6 => _progress,
                7 => _speed,
                _ => default,
            };
        }

        public float CreateDuration() {
            if (_blueprint == null) return 0f;

            _internalTween.tween = _blueprint.Read<ITween>(_token, 1);
            _internalTween.duration = Mathf.Max(_internalTween.tween?.CreateDuration() ?? 0f, 0f);

            return _internalTween.duration;
        }

        public async UniTask Play(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            if (_internalTween.tween == null) return;

            _speed = speed;
            _isFirstNotifyProgress = true;

            TweenExtensions.PlayAndForget(
                this,
                duration,
                (t, p) => t.ReportProgress(p),
                startProgress,
                speed,
                cancellationToken: cancellationToken
            );

            await _internalTween.tween.Play(duration, startProgress, speed, cancellationToken);

            // Notify OnFinished or OnCancelled
            _blueprint.Call(_token, cancellationToken.IsCancellationRequested && !_routeOnCancelledIntoOnFinished ? 5 : 4);
        }

        private void ReportProgress(float progress) {
            // Notify OnStart when ReportProgress is called first time
            // to save OnStart and OnCancelled calls order.
            if (_isFirstNotifyProgress) {
                _blueprint.Call(_token, 2);
                _isFirstNotifyProgress = false;
            }

            _progress = progress;
            _blueprint?.Call(_token, 5);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            meta.InvalidateNode(id, invalidateLinks: true);
        }
    }
}
