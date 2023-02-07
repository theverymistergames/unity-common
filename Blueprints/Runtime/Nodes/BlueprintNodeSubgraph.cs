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
        IBlueprintPortLinker,
        IBlueprintAssetValidator,
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

            foreach ((int nodeId, var nodeMeta) in nodesMap) {
                var node = nodeMeta.Node;
                var nodePorts = nodeMeta.Ports;

                for (int p = 0; p < nodePorts.Length; p++) {
                    var nodePort = nodePorts[p];
                    if (!nodePort.isExternalPort) continue;

                    // Drop external port if its linked port has no links
                    if (node is IBlueprintPortLinker linker) {
                        int linkedPortIndex = linker.GetLinkedPort(p);
                        var linkedPort = nodePorts[linkedPortIndex];

                        switch (linkedPort.mode) {
                            case Port.Mode.Input or Port.Mode.NonTypedInput or Port.Mode.Exit:
                                if (blueprintMeta.GetLinksFromNodePort(nodeId, linkedPortIndex).Count == 0) continue;
                                break;

                            case Port.Mode.Output or Port.Mode.NonTypedOutput or Port.Mode.Enter:
                                if (blueprintMeta.GetLinksToNodePort(nodeId, linkedPortIndex).Count == 0) continue;
                                break;
                        }
                    }

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

        public int GetLinkedPort(int port) {
            return port;
        }

        public void ResolveBlackboardSceneReferences(BlueprintAsset blueprint, Blackboard blackboard) {
            _host.ResolveBlackboardSceneReferences(blueprint, blackboard);
        }

        public void Compile(BlueprintNodeMeta nodeMeta) {
            _runtimeBlueprint = _blueprintAsset.CompileSubgraph(this, nodeMeta);
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            _blueprintAsset = BlueprintValidation.ValidateBlueprintAssetForSubgraph(blueprint, _blueprintAsset);

            if (_blueprintAsset == null) blueprint.BlueprintMeta.RemoveSubgraphReference(nodeId);
            else blueprint.BlueprintMeta.SetSubgraphReference(nodeId, _blueprintAsset);

            blueprint.BlueprintMeta.InvalidateNodePorts(nodeId, invalidateLinks: true);
        }
    }

}
