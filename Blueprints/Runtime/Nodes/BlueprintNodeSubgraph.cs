using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Subgraph", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeSubgraph :
        BlueprintNode,
        IBlueprintHost,
        IBlueprintEnter,
        IBlueprintOutput,
        IBlueprintValidatedNode,
        IBlueprintCompiledNode
    {
        [SerializeField] private BlueprintAsset _blueprintAsset;

        public MonoBehaviour Runner => _host.Runner;
        public Blackboard Blackboard => _blackboard;

        private RuntimeBlueprint _runtimeBlueprint;
        private Blackboard _blackboard;
        private IBlueprintHost _host;

        public override Port[] CreatePorts() {
            if (_blueprintAsset == null) return Array.Empty<Port>();

            var blueprintMeta = _blueprintAsset.BlueprintMeta;
            var nodesMap = blueprintMeta.NodesMap;

            var portSignatureSet = new HashSet<int>();
            var ports = new List<Port>();

            foreach (var nodeMeta in nodesMap.Values) {
                var nodePorts = nodeMeta.Ports;
                for (int p = 0; p < nodePorts.Length; p++) {
                    var nodePort = nodePorts[p];
                    if (!nodePort.isExternalPort) continue;

                    int portSignature = nodePort.GetSignature();
                    if (portSignatureSet.Contains(portSignature)) {
                        BlueprintValidation.ValidateExternalPortWithExistingSignature(_blueprintAsset, nodePort);
                        continue;
                    }

                    portSignatureSet.Add(portSignature);
                    ports.Add(nodePort.SetExternal(false));
                }
            }

            return ports.ToArray();
        }

        public override void OnInitialize(IBlueprintHost host) {
            _host = host;

            _blackboard = _blueprintAsset.Blackboard.Clone();
            ResolveBlackboardSceneReferences(_blueprintAsset, _blackboard);

            _runtimeBlueprint.Initialize(this);
        }

        public override void OnDeInitialize() {
            _runtimeBlueprint.DeInitialize();
        }

        public void OnEnterPort(int port) {
            CallPort(port);
        }

        public T GetPortValue<T>(int port) {
            return ReadPort<T>(port);
        }

        public void ResolveBlackboardSceneReferences(BlueprintAsset blueprint, Blackboard blackboard) {
            _host.ResolveBlackboardSceneReferences(blueprint, blackboard);
        }

        public void Compile(BlueprintNodeMeta nodeMeta) {
            _runtimeBlueprint = _blueprintAsset.CompileSubgraph(this, nodeMeta);
        }

        public void OnValidate(int nodeId, BlueprintAsset ownerAsset) {
            _blueprintAsset = BlueprintValidation.ValidateBlueprintAssetForSubgraph(ownerAsset, _blueprintAsset);

            if (_blueprintAsset == null) ownerAsset.BlueprintMeta.RemoveSubgraphReference(nodeId);
            else ownerAsset.BlueprintMeta.SetSubgraphReference(nodeId, _blueprintAsset);

            ownerAsset.BlueprintMeta.InvalidateNodePorts(nodeId, nodeInstance: this, invalidateLinks: true);
        }
    }

}
