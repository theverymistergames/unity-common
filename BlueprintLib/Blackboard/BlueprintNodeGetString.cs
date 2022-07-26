using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Get String", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeGetString : BlueprintNode, IBlueprintGetter<string> {

        [SerializeField] private string _property = "";

        private int _propertyId;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Output<string>()
        };

        protected override void OnInit() {
            _propertyId = Blackboard.StringToHash(_property);
        }

        string IBlueprintGetter<string>.Get(int port) => port switch {
            0 => blackboard.Get<string>(_propertyId),
            _ => ""
        };
        
    }

}