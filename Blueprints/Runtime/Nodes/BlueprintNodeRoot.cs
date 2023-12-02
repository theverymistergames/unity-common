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
    internal struct BlueprintNodeRoot : IBlueprintNode, IBlueprintEnter, IBlueprintOutput {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) { }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            ((RuntimeBlueprint) blueprint).ExternalCall(token.caller, port);
        }

        public T GetPortValue<T>(IBlueprint blueprint, NodeToken token, int port) {
            return ((RuntimeBlueprint) blueprint).ExternalRead<T>(token.caller, port);
        }
    }

}
