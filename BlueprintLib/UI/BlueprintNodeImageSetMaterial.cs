using System;
using MisterGames.Blueprints;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNodeMeta(Name = "Set Image Material", Category = "UI", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeImageSetMaterial : BlueprintNode, IBlueprintEnter {

        public override Port[] CreatePorts() => new[] {
            Port.Enter(),
            Port.Input<Image>("Image"),
            Port.Input<Material>("Material"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var image = ReadInputPort<Image>(1);
            var material = ReadInputPort<Material>(2);

            if (image != null) image.material = material;

            CallExitPort(3);
        }
    }

}
