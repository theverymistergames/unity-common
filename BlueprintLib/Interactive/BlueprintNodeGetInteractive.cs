using System;
using MisterGames.Blueprints;
using MisterGames.Interact.Core;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Get Interactive", Category = "Interactive", Color = BlueprintColors.Node.Data)]
    public sealed class BlueprintNodeGetInteractive : BlueprintNode, IBlueprintOutput<Interactive> {

        public override Port[] CreatePorts() => new[] {
            Port.Input<GameObject>("GameObject"),
            Port.Output<Interactive>("Interactive"),
        };

        public Interactive GetOutputPortValue(int port) {
            if (port != 1) return null;

            var gameObject = ReadInputPort<GameObject>(0);
            return gameObject == null ? null : gameObject.GetComponent<Interactive>();
        }
    }

}
