using System;
using MisterGames.Blueprints;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Random Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeRandomFloat : BlueprintNode, IBlueprintOutput<float> {

        [SerializeField] private float _from;
        [SerializeField] private float _to;

        public override Port[] CreatePorts() => new[] {
            Port.Func<float>(PortDirection.Output)
        };

        public float GetOutputPortValue(int port) => port switch {
            0 => Random.Range(_from, _to),
            _ => 0f
        };
    }

}
