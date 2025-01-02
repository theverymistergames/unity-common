using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
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
        IBlueprintOutput<IActorTween>,
        IBlueprintOutput<float>,
        IActorTween
    {
        [SerializeField] private bool _invertLayout;
        [SerializeField] private bool _routeOnCancelledIntoOnFinished;

        public float Duration { get; private set; }

        private IActorTween _selfTween;
        private IBlueprint _blueprint;
        private NodeToken _token;

        private float _speed;
        private float _progress;
        private bool _isFirstNotifyProgress;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(
                id,
                Port.Output<IActorTween>("Self")
                    .Capacity(PortCapacity.Single)
                    .Layout(_invertLayout ? PortLayout.Right : PortLayout.Left)
            );

            meta.AddPort(
                id,
                Port.Input<IActorTween>("Tween")
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

        IActorTween IBlueprintOutput<IActorTween>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
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

        public void CreateNextDuration() {
            if (_blueprint == null) {
                Duration = 0f;
                return;
            }

            _selfTween = _blueprint.Read<IActorTween>(_token, 1);
            _selfTween?.CreateNextDuration();

            Duration = Mathf.Max(_selfTween?.Duration ?? 0f, 0f);
        }

        public async UniTask Play(IActor context, float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            if (_selfTween == null || _blueprint == null) return;

            _speed = speed;
            _isFirstNotifyProgress = true;

            TweenExtensions.PlayAndForget(
                this,
                duration,
                (t, p, _) => t.ReportProgress(p),
                progressModifier: null,
                startProgress,
                speed,
                cancellationToken: cancellationToken
            );

            await _selfTween.Play(context, duration, startProgress, speed, cancellationToken);

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
