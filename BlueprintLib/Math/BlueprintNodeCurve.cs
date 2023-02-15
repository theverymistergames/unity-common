using System;
using MisterGames.Blueprints;
using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Curve", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeCurve : BlueprintNode, IBlueprintOutput<AnimationCurve> {

        [SerializeField] private EasingCurve _curve;

        public override Port[] CreatePorts() => new[] {
            Port.Output<AnimationCurve>()
        };

        public AnimationCurve GetOutputPortValue(int port) => port switch {
            0 => _curve.curve,
            _ => null
        };
    }

}
