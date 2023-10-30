using System;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    public class BlueprintSourceStart :
        BlueprintSource<BlueprintNodeStart2>,
        BlueprintSources.IStartCallback<BlueprintNodeStart2>,
        BlueprintSources.ICloneable { }

    [Serializable]
    [BlueprintNode(Name = "Start", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeStart2 : IBlueprintNode, IBlueprintStartCallback, IBlueprintCloneable {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Exit());
        }

        public void OnStart(IBlueprint blueprint, NodeId id) {
            blueprint.Call(id, 0);
        }
    }

}
