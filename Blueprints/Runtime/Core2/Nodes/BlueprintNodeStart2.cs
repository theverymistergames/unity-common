using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public class BlueprintSourceStart :
        BlueprintSource<BlueprintNodeStart2>,
        BlueprintSources.IInternalLink<BlueprintNodeStart2>,
        BlueprintSources.IStartCallback<BlueprintNodeStart2> { }

    [Serializable]
    [BlueprintNode(Name = "Start", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeStart2 : IBlueprintNode, IBlueprintStartCallback, IBlueprintInternalLink {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Exit());
            meta.AddPort(id, Port.Enter("On Start").External(true).Hidden(true));
        }

        public void OnStart(IBlueprint blueprint, NodeId id) {
            blueprint.Call(id, 0);
        }

        public void GetLinkedPorts(NodeId id, int port, out int index, out int count) {
            if (port == 1) {
                index = 0;
                count = 1;
            }

            index = -1;
            count = 0;
        }
    }

}
