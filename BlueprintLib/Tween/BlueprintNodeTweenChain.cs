using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.BlueprintLib.Tweens;
using MisterGames.Blueprints;
using MisterGames.Common.Attributes;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Tween Chain", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenChain : IBlueprintNode, IBlueprintOutput<IActorTween>, IActorTween {

        public float Duration { get; private set; }

        private readonly List<IActorTween> _nextTweens = new List<IActorTween>();
        private IActorTween _selfTween;
        private float _selfDuration;

        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<IActorTween>("Self").Capacity(PortCapacity.Single).Layout(PortLayout.Left));
            meta.AddPort(id, Port.Input<IActorTween>("Tween"));
            meta.AddPort(id, Port.Input<IActorTween>("Next").Capacity(PortCapacity.Multiple).Layout(PortLayout.Right));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _token = token;
            _blueprint = blueprint;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = null;
            _nextTweens.Clear();
        }

        IActorTween IBlueprintOutput<IActorTween>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;
            _blueprint = blueprint;

            return port == 0 ? this : default;
        }

        public void CreateNextDuration() {
            _nextTweens.Clear();

            if (_blueprint == null) {
                Duration = 0f;
                _selfDuration = 0f;
                return;
            }

            _selfTween = _blueprint.Read<IActorTween>(_token, 1);
            _selfTween?.CreateNextDuration();
            _selfDuration = Mathf.Max(_selfTween?.Duration ?? 0f, 0f);

            BlueprintTweenHelper.FetchLinkedTweens(_blueprint.GetLinks(_token, 2), dest: _nextTweens);
            float nextDuration = TweenExtensions.CreateNextDurationGroup(ExecuteMode.Parallel, _nextTweens);

            Duration = _selfDuration + nextDuration;
        }

        public UniTask Play(IActor context, float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return BlueprintTweenHelper.PlayTwoTweensAsSequence(
                data: (self: this, context),
                firstTask: (t, d, p, s, token) => t.self.PlaySelf(t.context, d, p, s, token),
                secondTask: (t, d, p, s, token) => t.self.PlayNext(t.context, d, p, s, token),
                firstDuration: _selfDuration,
                totalDuration: duration,
                startProgress,
                speed,
                cancellationToken
            );
        }

        private UniTask PlaySelf(IActor context, float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return _selfTween?.Play(context, duration, startProgress, speed, cancellationToken) ?? default;
        }
        
        private UniTask PlayNext(IActor context, float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return TweenExtensions.PlayParallel(context, _nextTweens, duration, startProgress, speed, cancellationToken);
        }
    }

}
