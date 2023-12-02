using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceSplitVector3 :
        BlueprintSource<BlueprintNodeSplitVector3>,
        BlueprintSources.IOutput<BlueprintNodeSplitVector3, float>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Random Float", Category = "Math", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeSplitVector3 : IBlueprintNode, IBlueprintOutput<float> {

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

}
