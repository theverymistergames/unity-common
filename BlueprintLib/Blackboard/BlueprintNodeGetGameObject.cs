using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Get GameObject", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeGetGameObject : BlueprintNode, IBlueprintGetter<GameObject>, IBlueprintGetter<string> {

        [SerializeField] private string _property = "";

        private int _propertyId;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Output<GameObject>()
        };

        protected override void OnInit() {
            _propertyId = Blackboard.StringToHash(_property);
        }

        GameObject IBlueprintGetter<GameObject>.Get(int port) => GetResult(port);

        string IBlueprintGetter<string>.Get(int port) => $"{GetResult(port)}";

        private GameObject GetResult(int port) => port switch {
            0 => blackboard.Get<GameObject>(_propertyId),
            _ => null
        };
        
    }

}