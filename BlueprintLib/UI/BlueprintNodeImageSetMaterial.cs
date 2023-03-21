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
            Port.Input<Image>("Images").Capacity(PortCapacity.Multiple),
            Port.Input<Material>("Material"),
            Port.Exit(),
        };

        public void OnEnterPort(int port) {
            if (port != 0) return;

            var material = Ports[2].Get<Material>();

            var links = Ports[1].links;
            for (int l = 0; l < links.Count; l++) {
                var link = links[l];

                if (link.Get<Image>() is { } image) {
                    image.material = material;
                    continue;
                }

                if (link.Get<Image[]>() is { } images) {
                    for (int i = 0; i < images.Length; i++) {
                        if (images[i] is { } im) im.material = material;
                    }
                }
            }

            Ports[3].Call();
        }
    }

}
