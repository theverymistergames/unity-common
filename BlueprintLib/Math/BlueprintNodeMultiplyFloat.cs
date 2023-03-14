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
            Port.Func<float>(PortDirection.Input, "A"),
            Port.Func<float>(PortDirection.Input, "B"),
            Port.Func<float>(PortDirection.Output)
        };

        public float GetOutputPortValue(int port) => port switch {
            2 => Ports[0].Get(_a) * Ports[1].Get(_b),
            _ => default,
        };
    }

}
