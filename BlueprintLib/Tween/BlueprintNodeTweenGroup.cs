using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.BlueprintLib.Tweens;
using MisterGames.Blueprints;
using MisterGames.Common.Attributes;
using MisterGames.Tweens.Core2;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Tween Group", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenGroup : IBlueprintNode, IBlueprintOutput<ITween>, ITween {

        [SerializeField] private TweenGroup.Mode _mode;

        private readonly List<(ITween tween, float duration)> _selfTweens = new List<(ITween tween, float duration)>();
        private readonly List<(ITween tween, float duration)> _nextTweens = new List<(ITween tween, float duration)>();

        private IBlueprint _blueprint;
        private NodeToken _token;
        private float _selfDuration;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<ITween>("Self").Capacity(PortCapacity.Single).Layout(PortLayout.Left));
            meta.AddPort(id, Port.Input<ITween>("Tweens").Capacity(PortCapacity.Multiple));
            meta.AddPort(id, Port.Input<ITween>("Next").Capacity(PortCapacity.Multiple).Layout(PortLayout.Right));
        }

        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _token = token;
            _blueprint = blueprint;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            _blueprint = null;
            _nextTweens.Clear();
        }

        ITween IBlueprintOutput<ITween>.GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            _token = token;
            _blueprint = blueprint;

            return port == 0 ? this : default;
        }

        public float CreateDuration() {
            if (_blueprint == null) return 0f;

            _nextTweens.Clear();
            _selfTweens.Clear();

            _selfDuration = BlueprintTweenHelper.CreateDurationFromLinkedTweens(
                _blueprint.GetLinks(_token, 1),
                _mode,
                dest: _selfTweens
            );

            float nextDuration = BlueprintTweenHelper.CreateDurationFromLinkedTweens(
                _blueprint.GetLinks(_token, 2),
                TweenGroup.Mode.Parallel,
                dest: _nextTweens
            );

            return _selfDuration + nextDuration;
        }

        public UniTask Play(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return BlueprintTweenHelper.PlayTwoTweensAsSequence(
                data: this,
                firstTask: (t, d, p, s, token) => t.PlaySelf(d, p, s, token),
                secondTask: (t, d, p, s, token) => t.PlayNext(d, p, s, token),
                firstDuration: _selfDuration,
                totalDuration: duration,
                startProgress,
                speed,
                cancellationToken
            );
        }

        private UniTask PlaySelf(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return _mode switch {
                TweenGroup.Mode.Sequential => TweenExtensions.PlaySequential(_selfTweens, duration, startProgress, speed, cancellationToken),
                TweenGroup.Mode.Parallel => TweenExtensions.PlayParallel(_selfTweens, duration, startProgress, speed, cancellationToken),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private UniTask PlayNext(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return TweenExtensions.PlayParallel(_nextTweens, duration, startProgress, speed, cancellationToken);
        }
    }

}
