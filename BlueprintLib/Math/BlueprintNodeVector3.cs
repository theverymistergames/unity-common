using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Vector3", Category = "Math", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeVector3 : BlueprintNode, IBlueprintOutput<Vector3> {

        [SerializeField] private Vector3 _vector;

        public override Port[] CreatePorts() => new[] {
            Port.Input<float>("X"),
            Port.Input<float>("Y"),
            Port.Input<float>("Z"),
            Port.Output<Vector3>(),
        };

        public Vector3 GetOutputPortValue(int port) => port switch {
            3 => new Vector3(Ports[0].Get(_vector.x), Ports[1].Get(_vector.y), Ports[2].Get(_vector.z)),
            _ => default
        };
    }

}
