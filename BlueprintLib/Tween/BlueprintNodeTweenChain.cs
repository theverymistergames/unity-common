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
    [BlueprintNode(Name = "Tween Chain", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenChain : IBlueprintNode, IBlueprintOutput<ITween>, ITween {

        private readonly List<(ITween tween, float duration)> _nextTweens = new List<(ITween tween, float duration)>();
        private (ITween tween, float duration) _internalTween;

        private IBlueprint _blueprint;
        private NodeToken _token;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<ITween>("Self").Capacity(PortCapacity.Single).Layout(PortLayout.Left));
            meta.AddPort(id, Port.Input<ITween>("Tween"));
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
            _internalTween.tween = _blueprint.Read<ITween>(_token, 1);
            _internalTween.duration = Mathf.Max(_internalTween.tween?.CreateDuration() ?? 0f, 0f);

            float nextDuration = BlueprintTweenHelper.CreateDurationFromLinkedTweens(
                _blueprint.GetLinks(_token, 2),
                TweenGroup.Mode.Parallel,
                dest: _nextTweens
            );

            return _internalTween.duration + nextDuration;
        }

        public UniTask Play(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return BlueprintTweenHelper.PlayTwoTweensAsSequence(
                data: this,
                firstTask: (t, d, p, s, token) => t.PlaySelf(d, p, s, token),
                secondTask: (t, d, p, s, token) => t.PlayNext(d, p, s, token),
                firstDuration: _internalTween.duration,
                totalDuration: duration,
                startProgress,
                speed,
                cancellationToken
            );
        }

        private UniTask PlaySelf(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return _internalTween.tween?.Play(duration, startProgress, speed, cancellationToken) ?? default;
        }
        
        private UniTask PlayNext(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return TweenExtensions.PlayParallel(_nextTweens, duration, startProgress, speed, cancellationToken);
        }
    }

}
