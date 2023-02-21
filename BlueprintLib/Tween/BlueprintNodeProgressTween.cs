using System;
using MisterGames.Blueprints;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "ProgressTween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProgressTween : BlueprintNode, IBlueprintOutput<ITween> {

        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] private AnimationCurve _curve;

        private readonly ProgressTween _tween = new ProgressTween();

        public override Port[] CreatePorts() => new[] {
            Port.Input<float>("Duration"),
            Port.Input<AnimationCurve>("Curve"),
            Port.Input<ITweenProgressAction>("Tween Progress Action"),
            Port.Output<ITween>("Tween"),
        };

        public ITween GetOutputPortValue(int port) {
            if (port != 3) return null;

            _tween.duration = Mathf.Max(0f, ReadInputPort(0, _duration));
            _tween.curve = ReadInputPort(1, _curve);
            _tween.action = ReadInputPort<ITweenProgressAction>(2);

            return _tween;
        }
    }

}
