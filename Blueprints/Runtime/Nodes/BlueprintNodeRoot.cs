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

        private RuntimeBlueprint2 _blueprint;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) { }

        public void OnInitialize(IBlueprint blueprint, NodeToken token) {
            _blueprint = (RuntimeBlueprint2) blueprint;
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token) {
            _blueprint = null;
        }

        public void OnEnterPort(NodeToken token, int port) {
            _blueprint.ExternalCall(token.caller, port);
        }

        public T GetPortValue<T>(NodeToken token, int port) {
            return _blueprint.ExternalRead<T>(token.caller, port);
        }
    }

}
