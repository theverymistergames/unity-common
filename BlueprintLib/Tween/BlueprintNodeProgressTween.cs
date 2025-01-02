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
using Random = UnityEngine.Random;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Progress Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProgressTween : IBlueprintNode, IBlueprintOutput<IActorTween>, IActorTween {

        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] [Min(0f)] private float _durationRandomAdd;
        [SerializeField] private AnimationCurve _curve;

        public float Duration { get; private set; }

        private readonly List<IActorTween> _nextTweens = new();
        private readonly List<ITweenProgressAction> _actions = new();

        private IBlueprint _blueprint;
        private NodeToken _token;
        private float _selfDuration;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<IActorTween>("Self").Capacity(PortCapacity.Single).Layout(PortLayout.Left));
            meta.AddPort(id, Port.Input<ITweenProgressAction>("Actions").Capacity(PortCapacity.Multiple));
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

            _selfDuration = _duration + Random.Range(-_durationRandomAdd, _durationRandomAdd);

            BlueprintTweenHelper.FetchLinkedTweens(_blueprint.GetLinks(_token, 2), dest: _nextTweens);
            float nextDuration = TweenExtensions.CreateNextDurationGroup(ExecuteMode.Parallel, _nextTweens);

            Duration = _selfDuration + nextDuration;
        }

        public UniTask Play(IActor context, float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return BlueprintTweenHelper.PlayTwoTweensAsSequence(
                data: (self: this, context),
                firstTask: (t, d, p, s, token) => t.self.PlaySelf(d, p, s, token),
                secondTask: (t, d, p, s, token) => t.self.PlayNext(t.context, d, p, s, token),
                firstDuration: _selfDuration,
                totalDuration: duration,
                startProgress,
                speed,
                cancellationToken
            );
        }

        private UniTask PlaySelf(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            FetchActions();

            return TweenExtensions.Play(
                this,
                duration,
                progressCallback: (t, p, _) => t.NotifyProgress(p),
                progressModifier: null,
                startProgress,
                speed,
                cancellationToken
            );
        }
        
        private UniTask PlayNext(IActor context, float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return TweenExtensions.PlayParallel(context, _nextTweens, duration, startProgress, speed, cancellationToken);
        }

        private void NotifyProgress(float progress) {
            float evaluatedProgress = _curve.Evaluate(progress);

            for (int i = 0; i < _actions.Count; i++) {
                _actions[i]?.OnProgressUpdate(evaluatedProgress);
            }
        }

        private void FetchActions() {
            _actions.Clear();
            var links = _blueprint.GetLinks(_token, 1);

            while (links.MoveNext()) {
                if (links.Read<ITweenProgressAction>() is { } a) {
                    _actions.Add(a);
                    continue;
                }

                if (links.Read<ITweenProgressAction[]>() is { } array) {
                    for (int i = 0; i < array.Length; i++) {
                        if (array[i] is {} a1) _actions.Add(a1);
                    }
                }
            }
        }
    }

}
