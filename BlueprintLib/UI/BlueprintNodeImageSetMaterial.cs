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

            var image = Ports[1].Get<Image>();
            var material = Ports[2].Get<Material>();

            image.material = material;

            Ports[3].Call();
        }
    }

}
