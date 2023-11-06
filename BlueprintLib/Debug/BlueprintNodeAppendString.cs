using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceAppendString :
        BlueprintSource<BlueprintNodeAppendString2>,
        BlueprintSources.IOutput<BlueprintNodeAppendString2, string> {}

    [Serializable]
    [BlueprintNode(Name = "Append String", Category = "Debug", Color = BlueprintColors.Node.Data)]
    public struct BlueprintNodeAppendString2 : IBlueprintNode, IBlueprintOutput2<string> {

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

    [Serializable]
    [BlueprintNodeMeta(Name = "Append String", Category = "Debug", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeAppendString : BlueprintNode, IBlueprintOutput<string> {

        [SerializeField] private string _a;
        [SerializeField] private string _b;

        public override Port[] CreatePorts() => new[] {
            Port.Input<string>("A"),
            Port.Input<string>("B"),
            Port.Output<string>()
        };

        public string GetOutputPortValue(int port) => port switch {
            2 => $"{Ports[0].Get(_a)}{Ports[1].Get(_b)}",
            _ => ""
        };
    }

}
