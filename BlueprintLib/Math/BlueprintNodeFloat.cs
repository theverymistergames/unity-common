using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeFloat : BlueprintNode, IBlueprintOutput<float> {

        [SerializeField] private float _value;

        public override Port[] CreatePorts() => new[] {
            Port.Output<float>(),
        };

        public float GetOutputPortValue(int port) => port switch {
            0 => _value,
            _ => default,
        };
    }

}
