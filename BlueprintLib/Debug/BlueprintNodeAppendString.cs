using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceAppendString :
        BlueprintSource<BlueprintNodeAppendString>,
        BlueprintSources.IOutput<BlueprintNodeAppendString, string>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Append String", Category = "Debug", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeAppendString : IBlueprintNode, IBlueprintOutput<string> {

        [SerializeField] private string _a;
        [SerializeField] private string _b;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Input<string>("A"));
            meta.AddPort(id, Port.Input<string>("B"));
            meta.AddPort(id, Port.Output<string>());
        }

        public string GetPortValue(IBlueprint blueprint, NodeToken token, int port) => port switch {
            2 => $"{blueprint.Read(token, 0, _a)}{blueprint.Read(token, 1, _b)}",
            _ => string.Empty,
        };
    }

}
