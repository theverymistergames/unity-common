using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Get Bool", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeGetBool : BlueprintNode, IBlueprintGetter<bool>, IBlueprintGetter<string> {

        [SerializeField] private string _property = "";

        private int _propertyId;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Output<bool>()
        };

        protected override void OnInit() {
            _propertyId = Blackboard.StringToHash(_property);
        }

        bool IBlueprintGetter<bool>.Get(int port) => GetResult(port);

        string IBlueprintGetter<string>.Get(int port) => $"{GetResult(port)}";

        private bool GetResult(int port) => port switch {
            0 => blackboard.Get<bool>(_propertyId),
            _ => false
        };
    }

}