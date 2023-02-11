using System;
using MisterGames.Blueprints;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;
using Tweens.Easing;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "ProgressTween", Category = "Tweens", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeProgressTween : BlueprintNode, IBlueprintOutput<ITween>, ITweenProgressAction {

        [SerializeField] [Min(0f)] private float _duration;
        [SerializeField] private EasingType _easingType = EasingType.Linear;
        [SerializeField] private bool _useCustomEasingCurve;
        [SerializeField] private AnimationCurve _customEasingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private readonly ProgressTween _tween = new ProgressTween();

        public override Port[] CreatePorts() => new[] {
            Port.Input<float>("Duration"),
            Port.Input<ITweenProgressAction>("Tween Progress Action"),
            Port.Output<ITween>("Tween"),
        };

        public override void OnInitialize(IBlueprintHost host) {
            _tween.easingType = _easingType;
            _tween.useCustomEasingCurve = _useCustomEasingCurve;
            _tween.customEasingCurve = _customEasingCurve;
        }

        public ITween GetOutputPortValue(int port) {
            if (port != 2) return null;

            _tween.duration = Mathf.Max(0f, ReadInputPort(0, _duration));
            _tween.action = ReadInputPort<ITweenProgressAction>(1, this);

            return _tween;
        }

        void ITweenProgressAction.Initialize(MonoBehaviour owner) { }

        void ITweenProgressAction.DeInitialize() { }

        void ITweenProgressAction.OnProgressUpdate(float progress) { }
    }

}
