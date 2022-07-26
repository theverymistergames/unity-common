using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Emit Blackboard Event", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeEmitBlackboardEvent : BlueprintNode, IBlueprintEnter {

        [SerializeField] private string _property = "";

        private BlackboardEvent _event;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Exit()
        };

        protected override void OnInit() {
            int propertyId = Blackboard.StringToHash(_property);
            _event = blackboard.Get<BlackboardEvent>(propertyId);
        }

        void IBlueprintEnter.Enter(int port) {
            if (port != 0) return;
            _event?.Emit();
            Call(port: 1);
        }
    }

}