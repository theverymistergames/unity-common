using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Random Bool", Category = "Data", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeRandomBool : BlueprintNode, IBlueprintGetter<bool>, IBlueprintGetter<string> {

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Output<bool>()
        };

        bool IBlueprintGetter<bool>.Get(int port) => GetResult(port);

        string IBlueprintGetter<string>.Get(int port) => $"{GetResult(port)}";

        private static bool GetResult(int port) => port switch {
            0 => Random.value > 0.5f,
            _ => false
        };
        
    }

}