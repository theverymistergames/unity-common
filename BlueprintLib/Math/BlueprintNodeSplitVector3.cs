using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceSplitVector3 :
        BlueprintSource<BlueprintNodeSplitVector32>,
        BlueprintSources.IOutput<BlueprintNodeSplitVector32, float>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Random Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeSplitVector32 : IBlueprintNode, IBlueprintOutput2<float> {

        [SerializeField] private Vector3 _vector;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<Vector3>());
            meta.AddPort(id, Port.Output<float>("x"));
            meta.AddPort(id, Port.Output<float>("y"));
            meta.AddPort(id, Port.Output<float>("z"));
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            1 => blueprint.Read(token, 0, _vector).x,
            2 => blueprint.Read(token, 0, _vector).y,
            3 => blueprint.Read(token, 0, _vector).z,
            _ => default
        };
    }

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
