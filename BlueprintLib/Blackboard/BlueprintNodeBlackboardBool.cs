using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Blackboard Bool", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeBlackboardBool : BlueprintNode, IBlueprintOutput<bool> {

        [SerializeField] private string _property;

        private Blackboard _blackboard;
        private int _propertyId;

        public override Port[] CreatePorts() => new[] {
            Port.Output<bool>()
        };

        public override void OnInitialize(IBlueprintHost host) {
            _blackboard = host.Blackboard;
            _propertyId = Blackboard.StringToHash(_property);
        }

        public bool GetOutputPortValue(int port) => port switch {
            0 => _blackboard.GetBool(_propertyId),
            _ => false
        };
    }

}
