using System;
using MisterGames.Blueprints.Runtime;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    internal class BlueprintSourceRoot :
        BlueprintSource<BlueprintNodeRoot>,
        BlueprintSources.IEnter<BlueprintNodeRoot>,
        BlueprintSources.IOutput<BlueprintNodeRoot>,
        BlueprintSources.ICloneable { }

    [Serializable]
    internal struct BlueprintNodeRoot : IBlueprintNode, IBlueprintEnter2, IBlueprintOutput2 {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) { }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            ((RuntimeBlueprint2) blueprint).ExternalCall(token.caller, port);
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port) {
            return ((RuntimeBlueprint2) blueprint).ExternalRead<T>(token.caller, port);
        }
    }

}
