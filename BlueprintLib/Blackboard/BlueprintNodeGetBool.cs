using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Get Bool", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeGetBool : BlueprintNode, IBlueprintOutput<bool> {

        [SerializeField] private string _property = "";

        private int _propertyId;

        public override Port[] CreatePorts() => new[] {
            Port.Output<bool>()
        };

        public override void OnInitialize(BlueprintRunner runner) {
            _propertyId = Blackboard.StringToHash(_property);
        }

        public bool GetPortValue(int port) => port switch {
            0 => false,//blackboard.Get<bool>(_propertyId),
            _ => false
        };
    }

}
