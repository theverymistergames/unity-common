﻿using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;

namespace MisterGames.Blueprints.Compile {

    public sealed class BlueprintCompiler {

        private readonly Dictionary<int, BlueprintNode> _runtimeNodesMap = new Dictionary<int, BlueprintNode>();
        private readonly Dictionary<int, List<BlueprintLink>> _externalPortLinksMap = new Dictionary<int, List<BlueprintLink>>();

        public RuntimeBlueprint Compile(BlueprintAsset blueprintAsset) {
            var blueprintMeta = blueprintAsset.BlueprintMeta;
            var nodesMetaMap = blueprintMeta.NodesMap;

            int nodesCount = nodesMetaMap.Count;
            int nodeIndex = 0;
            var runtimeNodes = nodesCount > 0 ? new BlueprintNode[nodesCount] : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();

            foreach ((int nodeId, var nodeMeta) in nodesMetaMap) {
                var runtimeNode = GetOrCreateNodeInstance(nodeId, nodeMeta);

                var ports = nodeMeta.Ports;
                int portsCount = ports.Length;
                runtimeNode.RuntimePorts ??= portsCount > 0 ? new RuntimePort[portsCount] : Array.Empty<RuntimePort>();

                for (int p = 0; p < portsCount; p++) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    BlueprintValidation.ValidatePort(blueprintAsset, nodeMeta, p);
#endif

                    var port = ports[p];
                    if (port.mode is Port.Mode.Enter or Port.Mode.Output or Port.Mode.NonTypedOutput) continue;

                    var links = blueprintMeta.GetLinksFromNodePort(nodeId, p);
                    int linksCount = links.Count;
                    var runtimeLinks = new List<RuntimeLink>(linksCount);

                    for (int l = 0; l < linksCount; l++) {
                        var link = links[l];

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        BlueprintValidation.ValidateLink(blueprintAsset, nodeMeta, p, nodesMetaMap[link.nodeId], link.portIndex);
#endif

                        runtimeLinks.Add(new RuntimeLink(GetOrCreateNodeInstance(link.nodeId, nodesMetaMap[link.nodeId]), link.portIndex));
                    }

                    runtimeNode.RuntimePorts[p] = new RuntimePort(runtimeLinks);
                }

                runtimeNodes[nodeIndex++] = runtimeNode;
            }

            _runtimeNodesMap.Clear();
/*
            for (int n = 0; n < runtimeNodes.Length; n++) {
                OptimizeLinkedNodes(runtimeNodes[n]);
            }
*/
            return new RuntimeBlueprint(runtimeNodes);
        }

        private static void OptimizeLinkedNodes(BlueprintNode node) {
            if (node is IBlueprintPortLinker) return;

            var runtimePorts = node.RuntimePorts;
            for (int p = 0; p < runtimePorts.Length; p++) {
                var runtimePort = runtimePorts[p];
                var runtimeLinks = runtimePort.links;

                if (runtimeLinks == null) continue;

                for (int l = runtimeLinks.Count - 1; l >= 0; l--) {
                    var link = runtimeLinks[l];

                    if (link.node is not IBlueprintPortLinker linker) {
                        OptimizeLinkedNodes(link.node);
                        continue;
                    }

                    var linkedPortLinks = link.node.RuntimePorts[linker.GetLinkedPort(link.port)].links;
                    runtimeLinks.InsertRange(l, linkedPortLinks);

                    l += linkedPortLinks.Count;
                    runtimeLinks.RemoveAt(l);
                }
            }
        }

        public RuntimeBlueprint CompileSubgraph(BlueprintAsset blueprintAsset, BlueprintNode subgraph, BlueprintNodeMeta subgraphMeta) {
            var blueprintMeta = blueprintAsset.BlueprintMeta;
            var nodesMetaMap = blueprintMeta.NodesMap;

            int nodesCount = nodesMetaMap.Count;
            int nodeIndex = 0;
            var runtimeNodes = nodesCount > 0 ? new BlueprintNode[nodesCount] : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();
            _externalPortLinksMap.Clear();

            var subgraphPorts = subgraphMeta.Ports;
            int subgraphPortsCount = subgraphPorts.Length;

            foreach ((int nodeId, var nodeMeta) in nodesMetaMap) {
                var runtimeNode = GetOrCreateNodeInstance(nodeId, nodeMeta);

                var ports = nodeMeta.Ports;
                int portsCount = ports.Length;
                runtimeNode.RuntimePorts ??= portsCount > 0 ? new RuntimePort[portsCount] : Array.Empty<RuntimePort>();

                for (int p = 0; p < portsCount; p++) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    BlueprintValidation.ValidatePort(blueprintAsset, nodeMeta, p);
#endif

                    var port = ports[p];
                    if (port.isExternalPort) {
                        int portSignature = port.GetSignature();

                        // External port is enter or output: add port address as link into external ports map
                        // to create links from matched subgraph node port to this external port.
                        if (port.mode is Port.Mode.Enter or Port.Mode.Output or Port.Mode.NonTypedOutput) {
                            var link = new BlueprintLink { nodeId = nodeId, portIndex = p };

                            if (_externalPortLinksMap.TryGetValue(portSignature, out var externalPortLinks)) {
                                externalPortLinks.Add(link);
                            }
                            else {
                                _externalPortLinksMap[portSignature] = new List<BlueprintLink> { link };
                            }
                            continue;
                        }

                        // External port is exit or input: create link from external port to matched subgraph node port.
                        int subgraphPortIndex = -1;
                        for (int i = 0; i < subgraphPortsCount; i++) {
                            if (subgraphPorts[i].GetSignature() != portSignature) continue;

                            subgraphPortIndex = i;
                            break;
                        }

                        runtimeNode.RuntimePorts[p] = new RuntimePort(new List<RuntimeLink>(1) { new RuntimeLink(subgraph, subgraphPortIndex) });
                        continue;
                    }

                    if (port.mode is Port.Mode.Enter or Port.Mode.Output or Port.Mode.NonTypedOutput) continue;

                    var links = blueprintMeta.GetLinksFromNodePort(nodeId, p);
                    int linksCount = links.Count;
                    var runtimeLinks = new List<RuntimeLink>(linksCount);

                    for (int l = 0; l < linksCount; l++) {
                        var link = links[l];

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        BlueprintValidation.ValidateLink(blueprintAsset, nodeMeta, p, nodesMetaMap[link.nodeId], link.portIndex);
#endif

                        runtimeLinks.Add(new RuntimeLink(GetOrCreateNodeInstance(link.nodeId, nodesMetaMap[link.nodeId]), link.portIndex));
                    }

                    runtimeNode.RuntimePorts[p] = new RuntimePort(runtimeLinks);
                }

                runtimeNodes[nodeIndex++] = runtimeNode;
            }

            subgraph.RuntimePorts ??= subgraphPortsCount > 0 ? new RuntimePort[subgraphPortsCount] : Array.Empty<RuntimePort>();

            // Create links from owner subgraph node ports to external nodes inside subgraph, if port is enter or output
            for (int p = 0; p < subgraphPortsCount; p++) {
                var port = subgraphPorts[p];

                if (port.mode is not (Port.Mode.Enter or Port.Mode.Output or Port.Mode.NonTypedOutput)) continue;

                var links = _externalPortLinksMap[port.GetSignature()];
                int linksCount = links.Count;
                var runtimeLinks = new List<RuntimeLink>(linksCount);

                for (int l = 0; l < linksCount; l++) {
                    var link = links[l];
                    runtimeLinks.Add(new RuntimeLink(GetOrCreateNodeInstance(link.nodeId, nodesMetaMap[link.nodeId]), link.portIndex));
                }

                subgraph.RuntimePorts[p] = new RuntimePort(runtimeLinks);
            }

            _externalPortLinksMap.Clear();
            _runtimeNodesMap.Clear();

            return new RuntimeBlueprint(runtimeNodes);
        }

        private BlueprintNode GetOrCreateNodeInstance(int nodeId, BlueprintNodeMeta nodeMeta) {
            if (_runtimeNodesMap.TryGetValue(nodeId, out var nodeInstance)) return nodeInstance;

            nodeInstance = nodeMeta.CreateNodeInstance();
            _runtimeNodesMap[nodeId] = nodeInstance;

            if (nodeInstance is IBlueprintCompiledNode compiledNode) compiledNode.Compile(nodeMeta);

            return nodeInstance;
        }
    }

}
