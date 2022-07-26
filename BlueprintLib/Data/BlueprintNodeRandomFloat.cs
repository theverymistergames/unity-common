using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Random Float", Category = "Data", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeRandomFloat : BlueprintNode, IBlueprintGetter<float>, IBlueprintGetter<string> {

        [SerializeField] private float _from;
        [SerializeField] private float _to;

        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Output<float>()
        };

        float IBlueprintGetter<float>.Get(int port) => GetResult(port);
        
        string IBlueprintGetter<string>.Get(int port) => $"{GetResult(port)}";

        private float GetResult(int port) => port switch {
            0 => Random.Range(_from, _to),
            _ => 0f
        };
    }

}