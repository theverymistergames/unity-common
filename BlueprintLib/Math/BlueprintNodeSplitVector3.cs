using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Split Vector3", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeSplitVector3 : BlueprintNode, IBlueprintOutput<float> {

        [SerializeField] private Vector3 _vector;

        public override Port[] CreatePorts() => new[] {
            Port.Input<Vector3>(),
            Port.Output<float>("x"),
            Port.Output<float>("y"),
            Port.Output<float>("z"),
        };

        public float GetOutputPortValue(int port) => port switch {
            1 => Ports[0].Get(_vector).x,
            2 => Ports[0].Get(_vector).y,
            3 => Ports[0].Get(_vector).z,
            _ => default
        };
    }

}
