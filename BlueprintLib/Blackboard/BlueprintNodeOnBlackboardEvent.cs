using System;
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

        public override void OnInitialize(IBlueprintHost host) {
            int propertyId = Blackboard.StringToHash(_property);
            _event = host.Blackboard.GetBlackboardEvent(propertyId);
            _event.OnEmit += OnEmit;
        }

        public override void OnDeInitialize() {
            if (_event != null) _event.OnEmit -= OnEmit;
        }

        private void OnEmit() {
            CallExitPort(0);
        }
    }

}
