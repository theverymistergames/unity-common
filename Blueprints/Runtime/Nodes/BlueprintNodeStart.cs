using System;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    public class BlueprintSourceStart :
        BlueprintSource<BlueprintNodeStart>,
        BlueprintSources.IStartCallback<BlueprintNodeStart>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Start", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeStart : IBlueprintNode, IBlueprintStartCallback {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Exit());
        }

        public void OnStart(IBlueprint blueprint, NodeToken token) {
            blueprint.Call(token, 0);
        }
    }

}
