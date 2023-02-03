using System;
using MisterGames.Blueprints;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Set Position", Category = "Actions", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeSetPosition : BlueprintNode, IBlueprintEnter {

        [SerializeField] private Vector3 _defaultPosition;

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<GameObject>("GameObject"),
            Port.Input<Vector3>("Position"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var gameObject = ReadInputPort<GameObject>(1);
            var position = ReadInputPort(2, _defaultPosition);
            gameObject.transform.position = position;

            CallExitPort(3);
        }
    }

}
