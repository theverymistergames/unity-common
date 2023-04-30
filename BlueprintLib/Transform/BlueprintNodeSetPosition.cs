using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Set Position", Category = "Transform", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeSetPosition : BlueprintNode, IBlueprintEnter {

        [SerializeField] private Vector3 _position;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<Transform>("Transform"),
            Port.Input<Vector3>("Position"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var transform = Ports[1].Get<Transform>();
            var position = Ports[2].Get(_position);

            transform.position = position;

            Ports[3].Call();
        }
    }

}
