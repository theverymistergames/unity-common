﻿using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "On Blackboard Event", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeOnBlackboardEvent : BlueprintNode {

        [SerializeField] private string _property;

        private BlackboardEvent _event;

        public override Port[] CreatePorts() => new[] {
            Port.Exit()
        };

        public override void OnInitialize(BlueprintRunner runner) {
            int propertyId = Blackboard.StringToHash(_property);
            _event = runner.Blackboard.Get<BlackboardEvent>(propertyId);
            _event.OnEmit += OnEmit;
        }

        public override void OnDeInitialize() {
            if (_event != null) _event.OnEmit -= OnEmit;
        }

        private void OnEmit() {
            CallPort(0);
        }
    }

}
