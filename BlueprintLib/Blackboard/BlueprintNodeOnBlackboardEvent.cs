using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "On Blackboard Event", Category = "Blackboard", Color = BlueprintColors.Node.Blackboard)]
    public sealed class BlueprintNodeOnBlackboardEvent : BlueprintNode {

        [SerializeField] private string _property = "";

        private BlackboardEvent _event;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Exit()
        };

        protected override void OnInit() {
            int propertyId = Blackboard.StringToHash(_property);
            _event = blackboard.Get<BlackboardEvent>(propertyId);
            _event.OnEmit += OnEmit;
        }

        protected override void OnTerminate() {
            if (_event != null) _event.OnEmit -= OnEmit;
        }

        private void OnEmit() {
            Call(port: 0);
        }
    }

}