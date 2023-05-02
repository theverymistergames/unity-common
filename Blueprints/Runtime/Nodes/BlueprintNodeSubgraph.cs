using System;
using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

#if UNITY_EDITOR
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;
#endif

namespace MisterGames.Blueprints.Nodes {

    [Serializable]
    [BlueprintNodeMeta(Name = "Subgraph", Category = "External", Color = BlueprintColors.Node.External)]
    public sealed class BlueprintNodeSubgraph :
        BlueprintNode,
        IBlueprintPortLinker,
        IBlueprintHost,
        IBlueprintCompiledNode

#if UNITY_EDITOR
        , IBlueprintAssetValidator
#endif

    {
        [SerializeField] private bool _useExternalRunner;

        [VisibleIf(nameof(_useExternalRunner))]
        [BlackboardProperty("_blackboard")]
        [SerializeField] private int _externalRunner;

        [SerializeField] private BlueprintAsset _blueprintAsset;

        public MonoBehaviour Runner => _host.Runner;
        public Blackboard Blackboard => _blackboard;

        private RuntimeBlueprint _runtimeBlueprint;
        private Blackboard _blackboard;
        private IBlueprintHost _host;

#if UNITY_EDITOR
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
                    if (!nodePort.IsExternal) continue;

                    // Drop external port if its linked port has no links
                    if (node is IBlueprintPortLinker linker) {
                        bool hasLinks = false;
                        int linkedPortIndex = linker.GetLinkedPorts(p, out int count);

                        for (int i = linkedPortIndex; i < count; i++) {
                            var linkedPort = nodePorts[i];

                            if (linkedPort.IsInput || !linkedPort.IsData) {
                                if (blueprintMeta.GetLinksFromNodePort(nodeId, i).Count > 0) {
                                    hasLinks = true;
                                    break;
                                }
                            }

                            if (!linkedPort.IsInput || !linkedPort.IsData) {
                                if (blueprintMeta.GetLinksToNodePort(nodeId, i).Count > 0) {
                                    hasLinks = true;
                                    break;
                                }
                            }
                        }

                        if (!hasLinks) continue;
                    }

                    int portSignature = nodePort.GetSignatureHashCode();

                    if (portSignatureSet.Contains(portSignature)) {
                        PortValidator.ValidateExternalPortWithExistingSignature(_blueprintAsset, nodePort);
                        continue;
                    }

                    portSignatureSet.Add(portSignature);
                    ports.Add(nodePort.External(false).Hidden(false));
                }
            }

            return ports.ToArray();
        }

        public void ValidateBlueprint(BlueprintAsset blueprint, int nodeId) {
            _blueprintAsset = SubgraphValidator.ValidateBlueprintAssetForSubgraph(blueprint, _blueprintAsset);

            if (_blueprintAsset == null || _useExternalRunner) {
                blueprint.BlueprintMeta.RemoveSubgraphReference(nodeId);
            }
            else {
                blueprint.BlueprintMeta.SetSubgraphReference(nodeId, _blueprintAsset);
            }

            blueprint.BlueprintMeta.InvalidateNodePorts(blueprint, nodeId, invalidateLinks: true);
        }
#else
        public override Port[] CreatePorts() => null;
#endif

        public int GetLinkedPorts(int port, out int count) {
            count = 1;
            return port;
        }

        public override void OnInitialize(IBlueprintHost host) {
            _host = _useExternalRunner
                ? host.Blackboard.Get<BlueprintRunner>(_externalRunner)
                : host;

            _blackboard = GetBlackboard(_blueprintAsset);
            _runtimeBlueprint.Initialize(this);
        }

        public override void OnDeInitialize() {
            _runtimeBlueprint.DeInitialize();
        }

        public Blackboard GetBlackboard(BlueprintAsset blueprint) {
            return _host.GetBlackboard(blueprint);
        }

        public void Compile(Port[] ports) {
            _runtimeBlueprint = _blueprintAsset.CompileSubgraph(this, ports);
        }
    }

}
