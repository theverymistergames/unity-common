using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.BlueprintLib.Tweens;
using MisterGames.Blueprints;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using MisterGames.Tweens;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [SubclassSelectorIgnore]
    [BlueprintNode(Name = "Progress Tween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProgressTween : IBlueprintNode, IBlueprintOutput<ITween>, ITween {

        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] [Min(0f)] private float _durationRandomAdd;
        [SerializeField] private AnimationCurve _curve;

        public float Duration { get; private set; }

        private readonly List<ITween> _nextTweens = new List<ITween>();
        private readonly List<ITweenProgressAction> _actions = new List<ITweenProgressAction>();

        private IBlueprint _blueprint;
        private NodeToken _token;
        private float _selfDuration;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Output<ITween>("Self").Capacity(PortCapacity.Single).Layout(PortLayout.Left));
            meta.AddPort(id, Port.Input<ITweenProgressAction>("Actions").Capacity(PortCapacity.Multiple));
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
            _nextTweens.Clear();

            if (_blueprint == null) {
                Duration = 0f;
                _selfDuration = 0f;
                return;
            }

            _selfDuration = _duration + Random.Range(-_durationRandomAdd, _durationRandomAdd);

            BlueprintTweenHelper.FetchLinkedTweens(_blueprint.GetLinks(_token, 2), dest: _nextTweens);
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
            FetchActions();

            return TweenExtensions.Play(
                this,
                duration,
                progressCallback: (t, p) => t.NotifyProgress(p),
                startProgress,
                speed,
                PlayerLoopStage.Update,
                cancellationToken
            );
        }
        
        private UniTask PlayNext(float duration, float startProgress, float speed, CancellationToken cancellationToken = default) {
            return TweenExtensions.PlayParallel(_nextTweens, duration, startProgress, speed, cancellationToken);
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
