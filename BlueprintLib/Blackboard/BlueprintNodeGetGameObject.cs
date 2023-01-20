using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Get GameObject", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeGetGameObject : BlueprintNode, IBlueprintOutput<GameObject> {

        [SerializeField] private string _property = "";

        private int _propertyId;

        public override Port[] CreatePorts() => new[] {
            Port.Output<GameObject>()
        };

        public override void OnInitialize(BlueprintRunner runner) {
            _propertyId = Blackboard.StringToHash(_property);
        }

        public GameObject GetPortValue(int port) => port switch {
            0 => null,//blackboard.Get<GameObject>(_propertyId),
            _ => null
        };
    }

}
