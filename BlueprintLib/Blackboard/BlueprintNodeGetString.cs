using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Get String", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeGetString : BlueprintNode, IBlueprintOutput<string> {

        [SerializeField] private string _property;

        private RuntimeBlackboard _blackboard;
        private int _propertyId;

        public override Port[] CreatePorts() => new[] {
            Port.Output<string>()
        };

        public override void OnInitialize(BlueprintRunner runner) {
            _blackboard = runner.Blackboard;
            _propertyId = Blackboard.StringToHash(_property);
        }

        public string GetPortValue(int port) => port switch {
            0 => _blackboard.Get<string>(_propertyId),
            _ => string.Empty
        };
    }

}
