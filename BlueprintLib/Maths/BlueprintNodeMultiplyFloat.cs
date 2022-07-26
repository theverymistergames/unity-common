using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Multiply Float", Category = "Maths", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeMultiplyFloat : BlueprintNode, IBlueprintGetter<float>, IBlueprintGetter<string> {

        [SerializeField] private float _a;
        [SerializeField] private float _b;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Input<float>("A"),
            Port.Input<float>("B"),
            Port.Output<float>()
        };

        float IBlueprintGetter<float>.Get(int port) => GetResult(port);

        string IBlueprintGetter<string>.Get(int port) => $"{GetResult(port)}";

        private float GetResult(int port) => port switch {
            2 => Read(0, _a) * Read(1, _b),
            _ => 0f
        };
        
    }

}