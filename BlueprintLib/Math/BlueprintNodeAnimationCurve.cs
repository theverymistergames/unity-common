using System;
using MisterGames.Blueprints;
using Tweens.Easing;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Animation Curve", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeAnimationCurve : BlueprintNode, IBlueprintOutput<AnimationCurve> {

        [SerializeField] private bool _useCustomCurve;
        [SerializeField] private EasingType _easing = EasingType.Linear;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public override Port[] CreatePorts() => new[] {
            Port.Output<AnimationCurve>()
        };

        public AnimationCurve GetOutputPortValue(int port) => port switch {
            0 => _curve,
            _ => null
        };

        public override void OnValidate() {
            if (!_useCustomCurve) _curve = _easing.ToAnimationCurve();
        }
    }

}
