using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceVector3 :
        BlueprintSource<BlueprintNodeVector32>,
        BlueprintSources.IOutput<BlueprintNodeVector32, Vector3>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Vector3", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeVector32 : IBlueprintNode, IBlueprintOutput2<Vector3> {

        [SerializeField] private Vector3 _vector;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<float>("X"));
            meta.AddPort(id, Port.Input<float>("Y"));
            meta.AddPort(id, Port.Input<float>("Z"));
            meta.AddPort(id, Port.Output<Vector3>());
        }

        public Vector3 GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            3 => new Vector3(
                blueprint.Read(token, 0, _vector.x),
                blueprint.Read(token, 1, _vector.y),
                blueprint.Read(token, 2, _vector.z)
            ),
            _ => default
        };
    }

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
