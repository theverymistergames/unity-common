using System.Collections.Generic;
using MisterGames.Blueprints;
using MisterGames.Blueprints.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [BlueprintNode(Name = "Set Position", Category = "Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeSetPosition : BlueprintNode, IBlueprintEnter {

        [SerializeField] private Vector3 _defaultPosition;
        
        protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
            Port.Enter(),
            Port.Input<GameObject>("GameObject"),
            Port.Input<Vector3>("Position"),
            Port.Exit(),
        };

        void IBlueprintEnter.Enter(int port) {
            if (port != 0) return;
            
            var gameObject = Read<GameObject>(1);
            var position = Read(2, _defaultPosition);
            gameObject.transform.position = position;
            
            Call(port: 3);
        }

    }

}