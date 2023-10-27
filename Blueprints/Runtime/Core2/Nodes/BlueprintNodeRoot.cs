using System;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    internal class BlueprintSourceRoot : BlueprintSource<BlueprintNodeRoot>,
        BlueprintSources.IEnter<BlueprintNodeRoot>,
        BlueprintSources.IOutput<BlueprintNodeRoot> { }

    [Serializable]
    internal struct BlueprintNodeRoot : IBlueprintNode, IBlueprintEnter2, IBlueprintOutput2 {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) { }

        public void OnEnterPort(IBlueprint blueprint, NodeId id, int port) {
            blueprint.Call(id, port);
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeId id, int port) {
            return blueprint.Read<T>(id, port);
        }
    }

}
