using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    [BlueprintNodeMeta(Name = "Core2.Subgraph", Category = "Core2.External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeSubgraph :
        BlueprintNode,
        IBlueprintEnter,
        IBlueprintOutput,
        IBlueprintCompiledNode,
        IBlueprintValidatedNode
    {

        [SerializeField] private BlueprintAsset _blueprintAsset;

        private RuntimeBlueprint _runtimeBlueprint;

        public override Port[] CreatePorts() {
            if (_blueprintAsset == null) return Array.Empty<Port>();

            var blueprintMeta = _blueprintAsset.BlueprintMeta;
            var nodesMap = blueprintMeta.Nodes;

            var externalPortLinks = blueprintMeta.ExternalPortLinks;
            int portsCount = externalPortLinks.Count;

            var ports = portsCount > 0 ? new Port[portsCount] : Array.Empty<Port>();

            for (int p = 0; p < portsCount; p++) {
                var link = externalPortLinks[p];
                ports[p] = nodesMap[link.nodeId].Ports[link.portIndex].SetExternal(false);
            }
            
            return ports;
        }

        public void OnEnterPort(int port) {
            CallPort(port);
        }

        public T GetPortValue<T>(int port) {
            return ReadPort<T>(port);
        }

        public void Compile(BlueprintNodeMeta nodeMeta) {
            _runtimeBlueprint = _blueprintAsset.CompileSubgraph(this, nodeMeta);
        }

        public void OnValidate(int nodeId, BlueprintAsset owner) {
            if (_blueprintAsset == owner) {
                Debug.LogWarning($"Subgraph node can not execute its owner BlueprintAsset {owner.name}");
                _blueprintAsset = null;
            }

            owner.BlueprintMeta.Invalidate(nodeId);
        }
    }

}
