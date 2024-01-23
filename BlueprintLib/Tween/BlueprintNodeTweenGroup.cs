using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.BlueprintLib.Tweens;
using MisterGames.Blueprints;
using MisterGames.Common.Attributes;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Tween Group", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeTweenGroup : IBlueprintNode, IBlueprintOutput<ITween>, ITween {

        [SerializeField] private TweenGroup.Mode _mode;

        public float Duration { get; private set; }

        private readonly List<ITween> _selfTweens = new List<ITween>();
        private readonly List<ITween> _nextTweens = new List<ITween>();

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

        public void CreateNextDuration() {
            if (_blueprint == null) {
                Duration = 0f;
                _selfDuration = 0f;
                return;
            }

            _selfTweens.Clear();
            _nextTweens.Clear();

            BlueprintTweenHelper.FetchLinkedTweens(_blueprint.GetLinks(_token, 1), dest: _selfTweens);
            BlueprintTweenHelper.FetchLinkedTweens(_blueprint.GetLinks(_token, 2), dest: _nextTweens);

            _selfDuration = TweenExtensions.CreateNextDurationGroup(_mode, _selfTweens);
            float nextDuration = TweenExtensions.CreateNextDurationGroup(TweenGroup.Mode.Parallel, _nextTweens);

            Duration = _selfDuration + nextDuration;
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
            return TweenExtensions.PlayGroup(_mode, _selfTweens, duration, startProgress, speed, cancellationToken);
        }

        private UniTask PlayNext(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return TweenExtensions.PlayParallel(_nextTweens, duration, startProgress, speed, cancellationToken);
        }
    }

}
