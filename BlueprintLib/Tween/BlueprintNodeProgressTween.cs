using System;
using MisterGames.Blueprints;
using MisterGames.Common.Easing;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "ProgressTween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProgressTween : BlueprintNode, IBlueprintOutput<ITween>, ITweenProgressAction {

        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] private EasingCurve _curve;

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

            _tween.useCustomEasingCurve = true;
            _tween.customEasingCurve = ReadInputPort(1, _curve.curve);

            _tween.action = ReadInputPort<ITweenProgressAction>(2, this);

            return _tween;
        }

        void ITweenProgressAction.Initialize(MonoBehaviour owner) { }

        void ITweenProgressAction.DeInitialize() { }

        void ITweenProgressAction.OnProgressUpdate(float progress) { }
    }

}
