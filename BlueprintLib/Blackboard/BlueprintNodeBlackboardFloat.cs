using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Blackboard Float", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeBlackboardFloat : BlueprintNode, IBlueprintOutput<float> {

        [SerializeField] private string _property;

        private Blackboard _blackboard;
        private int _propertyId;

        public override Port[] CreatePorts() => new[] {
            Port.Output<float>()
        };

        public override void OnInitialize(IBlueprintHost host) {
            _blackboard = host.Blackboard;
            _propertyId = Blackboard.StringToHash(_property);
        }

        public float GetOutputPortValue(int port) => port switch {
            0 => _blackboard.GetFloat(_propertyId),
            _ => 0f
        };
    }

}
