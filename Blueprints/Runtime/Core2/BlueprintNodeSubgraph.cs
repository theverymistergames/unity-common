using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    [BlueprintNodeMeta(Name = "Core2.Subgraph", Category = "Core2.Exposed", Color = BlueprintColors.Node.Flow)]
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

            
            var subgraphPorts = subgraph.Ports;
            var ports = new List<Port>();
            
            for (int i = 0; i < subgraphPorts.Length; i++) {
                var subgraphPort = subgraphPorts[i];
                if (!subgraphPort.IsExposed) continue;
                
                ports.Add(subgraphPort.NotExposed());
            }
            
            return ports;
        }

        public void OnEnterPort(int port) {
            CallPort(port);
        }

        public T GetPortValue<T>(int port) {
            return ReadPort<T>(port);
        }

        public void Compile() {
            _runtimeBlueprint = _blueprintAsset.Compile();
        }

        public void OnValidate(int nodeId, BlueprintAsset owner) {
            if (_blueprintAsset == owner) {
                Debug.LogWarning($"Subgraph node can not execute its owner BlueprintAsset {owner.name}");
                _blueprintAsset = null;
            }

            owner.BlueprintMeta.InvalidateNode(nodeId);
        }

        public BlueprintNode Compile(int port) {

        }
    }

}
