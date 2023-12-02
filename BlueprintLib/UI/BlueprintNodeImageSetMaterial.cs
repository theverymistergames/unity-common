using System;
using MisterGames.Blueprints;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.BlueprintLib {

    [Serializable]
    public class BlueprintSourceImageSetMaterial :
        BlueprintSource<BlueprintNodeImageSetMaterial>,
        BlueprintSources.IEnter<BlueprintNodeImageSetMaterial>,
        BlueprintSources.ICloneable {}

    [Serializable]
    [BlueprintNode(Name = "Set Image Material", Category = "UI", Color = BlueprintColors.Node.Actions)]
    public struct BlueprintNodeImageSetMaterial : IBlueprintNode, IBlueprintEnter {

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

}
