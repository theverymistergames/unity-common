using System;
using System.Collections.Generic;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    [BlueprintNode(Name = "Subgraph", Category = "External", Color = BlueprintColors.Node.External)]
    public struct BlueprintNodeSubgraph : IBlueprintNode, IBlueprintCompiled {

        [SerializeField] private bool _useExternalRunner;

        [VisibleIf(nameof(_useExternalRunner))]
        [BlackboardProperty("_blackboard")]
        [SerializeField] private int _externalRunner;

        [SerializeField] private BlueprintAsset2 _blueprint;

        public MonoBehaviour Runner => _host.Runner;
        public Blackboard Blackboard => _blackboard;

        private Blackboard _blackboard;
        private IBlueprintHost2 _host;

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            if (_blueprint == null) return;

            var subgraphMeta = _blueprint.BlueprintMeta;
            var nodes = subgraphMeta.Nodes;

            var portSignatureSet = new HashSet<int>();

            foreach (var subgraphNodeId in nodes) {
                int portCount = subgraphMeta.GetPortCount(subgraphNodeId);

                for (int p = 0; p < portCount; p++) {
                    var port = subgraphMeta.GetPort(subgraphNodeId, p);
                    if (!port.IsExternal()) continue;

                    // Drop external port if its linked port has no links
                    if (node is IBlueprintPortLinker linker) {
                        bool hasLinks = false;
                        int linkedPortIndex = linker.GetLinkedPorts(p, out int count);

                        for (int i = linkedPortIndex; i < count; i++) {
                            var linkedPort = nodePorts[i];

                            if (linkedPort.IsInput() || !linkedPort.IsData()) {
                                if (meta.GetLinksFromNodePort(nodeId, i).Count > 0) {
                                    hasLinks = true;
                                    break;
                                }
                            }

                            if (!linkedPort.IsInput() || !linkedPort.IsData()) {
                                if (meta.GetLinksToNodePort(nodeId, i).Count > 0) {
                                    hasLinks = true;
                                    break;
                                }
                            }
                        }

                        if (!hasLinks) continue;
                    }

                    int portSignature = port.GetSignature();

                    if (portSignatureSet.Contains(portSignature)) {
                        PortValidator2.ValidateExternalPortWithExistingSignature(subgraphMeta, port);
                        continue;
                    }

                    portSignatureSet.Add(portSignature);
                    meta.AddPort(id, port.External(false).Hidden(false));
                }
            }
        }

        public void OnInitialize(IBlueprint blueprint, NodeId id) {
            _host = _useExternalRunner
                ? blueprint.Host.Blackboard.Get<BlueprintRunner2>(_externalRunner)
                : blueprint.Host;

            _blackboard = _host.GetBlackboard(_blueprint);
        }

        public void Compile(IBlueprintFactory factory, BlueprintCompileData data) {
            if (_blueprint != null) _blueprint.CompileSubgraph(factory, data);
        }

        public void OnValidate(IBlueprintMeta meta, NodeId id) {
            SubgraphValidator2.ValidateSubgraphAsset(meta, ref _blueprint);

            if (_blueprint == null || _useExternalRunner) {
                meta.RemoveSubgraph(id);
            }
            else {
                meta.SetSubgraph(id, _blueprint);
            }

            meta.InvalidateNode(id, invalidateLinks: true);
        }

        public Blackboard GetBlackboard(BlueprintAsset blueprint) {
            return ;
        }
    }

}
