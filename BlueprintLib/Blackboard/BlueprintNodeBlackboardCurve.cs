using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Blackboard Curve", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeBlackboardCurve : BlueprintNode, IBlueprintOutput<AnimationCurve> {

        [SerializeField] private string _property;

        private Blackboard _blackboard;
        private int _propertyId;

        public override Port[] CreatePorts() => new[] {
            Port.Output<AnimationCurve>()
        };

        public override void OnInitialize(IBlueprintHost host) {
            _blackboard = host.Blackboard;
            _propertyId = Blackboard.StringToHash(_property);
        }

        public AnimationCurve GetOutputPortValue(int port) => port switch {
            0 => _blackboard.GetCurve(_propertyId).curve,
            _ => null,
        };
    }

}
