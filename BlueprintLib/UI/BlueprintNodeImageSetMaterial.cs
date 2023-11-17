using System;
using MisterGames.Blueprints;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceImageSetMaterial :
        BlueprintSource<BlueprintNodeImageSetMaterial2>,
        BlueprintSources.IEnter<BlueprintNodeImageSetMaterial2>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Set Image Material", Category = "UI", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeImageSetMaterial2 : IBlueprintNode, IBlueprintEnter2 {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter());
            meta.AddPort(id, Port.Input<Image>());
            meta.AddPort(id, Port.Input<Material>());
            meta.AddPort(id, Port.Exit());
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            if (port != 0) return;

            var image = blueprint.Read<Image>(token, 1);
            var material = blueprint.Read<Material>(token, 2);

            image.material = material;

            blueprint.Call(token, 3);
        }
    }

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
