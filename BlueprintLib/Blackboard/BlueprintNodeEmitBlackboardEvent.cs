using System;
using MisterGames.Blueprints;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Emit Blackboard Event", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeEmitBlackboardEvent : BlueprintNode, IBlueprintEnter {

        [SerializeField] private string _property = "";

        private BlackboardEvent _event;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Exit()
        };

        public override void OnInitialize(BlueprintRunner runner) {
            int propertyId = Blackboard.StringToHash(_property);
            //_event = blackboard.Get<BlackboardEvent>(propertyId);
        }

        public void OnEnterPort(int port) {
            if (port != 0) return;

            _event?.Emit();
            CallPort(1);
        }
    }

}
