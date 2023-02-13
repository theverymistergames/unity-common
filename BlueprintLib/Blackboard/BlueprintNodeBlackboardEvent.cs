using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Blackboard Event", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeBlackboardEvent : BlueprintNode, IBlueprintEnter {

        [SerializeField] private string _property;

        private BlackboardEvent _event;

        public override Port[] CreatePorts() => new[] {
            Port.Enter("Emit"),
            Port.Exit("On Emit")
        };

        public override void OnInitialize(IBlueprintHost host) {
            int propertyId = Blackboard.StringToHash(_property);
            _event = host.Blackboard.GetBlackboardEvent(propertyId);
            _event.OnEmit += OnEmit;
        }

        public override void OnDeInitialize() {
            if (_event != null) _event.OnEmit -= OnEmit;
        }

        public void OnEnterPort(int port) {
            if (port != 0) return;

            _event.Emit();
        }

        private void OnEmit() {
            CallExitPort(1);
        }
    }

}
