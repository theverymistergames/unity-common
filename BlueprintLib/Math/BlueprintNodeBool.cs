using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Bool", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeBool : BlueprintNode, IBlueprintOutput<bool> {

        [SerializeField] private bool _value;

        public override Port[] CreatePorts() => new[] {
            Port.Output<bool>(),
        };

        public bool GetOutputPortValue(int port) => port switch {
            0 => _value,
            _ => default,
        };
    }

}
