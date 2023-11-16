using System;
using MisterGames.Blueprints;

namespace Core {

    [Serializable]
    public sealed class BlueprintSourceTest3 :
        BlueprintSource<BlueprintNodeTest3>,
        BlueprintSources.IEnter<BlueprintNodeTest3>,
        BlueprintSources.IOutput<BlueprintNodeTest3, int> { }

    [Serializable]
    public struct BlueprintNodeTest3 : IBlueprintNode, IBlueprintEnter2, IBlueprintOutput2<int> {

        public int pickedPort;

        public void OnSetDefaults(IBlueprintMeta meta, NodeId id) {
            pickedPort = -1;
        }

        public void CreatePorts(IBlueprintMeta meta, NodeId id) { }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            pickedPort = port;
        }

        public int GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return pickedPort;
        }
    }

}
