using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Set Position", Category = "Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeSetPosition : BlueprintNode, IBlueprintEnter {

        [SerializeField] private Vector3 _position;

        public override Port[] CreatePorts() => new[] {
            Port.Action(PortDirection.Input),
            Port.Func<Transform>(PortDirection.Input, "Transform"),
            Port.Func<Vector3>(PortDirection.Input, "Position"),
            Port.Action(PortDirection.Output),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var transform = Ports[1].Get<Transform>();
            var position = Ports[2].Get(_position);

            if (transform != null) transform.position = position;

            Ports[3].Call();
        }
    }

}
