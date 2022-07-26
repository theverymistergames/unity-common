using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Append String", Category = "Data", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeAppendString : BlueprintNode, IBlueprintGetter<string> {

        [SerializeField] private string _a;
        [SerializeField] private string _b;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Input<string>("A"),
            Port.Input<string>("B"),
            Port.Output<string>()
        };

        string IBlueprintGetter<string>.Get(int port) => port switch {
            2 => $"{Read(0, _a)}{Read(1, _b)}",
            _ => ""
        };
        
    }

}