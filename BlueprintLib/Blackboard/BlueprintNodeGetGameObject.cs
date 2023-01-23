﻿using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Get GameObject", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeGetGameObject : BlueprintNode, IBlueprintOutput<GameObject> {

        [SerializeField] private string _property;

        private Blackboard _blackboard;
        private int _propertyId;

        public override Port[] CreatePorts() => new[] {
            Port.Output<GameObject>()
        };

        public override void OnInitialize(IBlueprintHost host) {
            _blackboard = host.Blackboard;
            _propertyId = Blackboard.StringToHash(_property);
        }

        public GameObject GetPortValue(int port) => port switch {
            0 => _blackboard.GetGameObject(_propertyId),
            _ => null
        };
    }

}
