using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Set Euler Angles", Category = "Transform", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeSetEulerAngles : BlueprintNode, IBlueprintEnter {

        [SerializeField] private Vector3 _eulers;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<Transform>(),
            Port.Input<Vector3>("Eulers"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var transform = Ports[1].Get<Transform>();
            var eulers = Ports[2].Get(_eulers);

            transform.eulerAngles = eulers;

            Ports[3].Call();
        }
    }

}
