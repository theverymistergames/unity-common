using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Multiply Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeMultiplyFloat : BlueprintNode, IBlueprintOutput<float> {

        [SerializeField] private float _a;
        [SerializeField] private float _b;

        public override Port[] CreatePorts() => new[] {
            Port.Input<float>("A"),
            Port.Input<float>("B"),
            Port.Output<float>()
        };

        public float GetOutputPortValue(int port) => port switch {
            2 => ReadInputPort(0, _a) * ReadInputPort(1, _b),
            _ => 0f
        };
    }

}
