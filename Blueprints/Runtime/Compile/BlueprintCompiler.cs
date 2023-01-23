using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;

namespace MisterGames.Blueprints.Compile {

    public sealed class BlueprintCompiler {

        private readonly Dictionary<int, BlueprintNode> _runtimeNodesMap = new Dictionary<int, BlueprintNode>();

        public RuntimeBlueprint Compile(BlueprintAsset blueprintAsset) {
            var blueprintMeta = blueprintAsset.BlueprintMeta;
            var nodesMetaMap = blueprintMeta.NodesMap;
            int nodesCount = nodesMetaMap.Count;

            int nodeIndex = 0;
            var runtimeNodes = nodesCount > 0
                ? new BlueprintNode[nodesCount]
                : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();

            foreach ((int nodeId, var nodeMeta) in nodesMetaMap) {
                var runtimeNode = GetOrCreateNodeInstance(nodeId, nodeMeta);

                var ports = nodeMeta.Ports;
                int portsCount = ports.Count;

                var runtimePorts = portsCount > 0
                    ? new RuntimePort[portsCount]
                    : Array.Empty<RuntimePort>();

                for (int p = 0; p < portsCount; p++) {
#if DEBUG || UNITY_EDITOR
                    BlueprintValidation.ValidatePort(blueprintAsset, nodeMeta, p);
#endif

                    var links = blueprintMeta.GetLinksFromNodePort(nodeId, p);
                    int linksCount = links.Count;

                    var runtimeLinks = linksCount > 0
                        ? new RuntimeLink[linksCount]
                        : Array.Empty<RuntimeLink>();

                    for (int l = 0; l < linksCount; l++) {
                        var link = links[l];
                        var linkedNodeMeta = nodesMetaMap[link.nodeId];

#if DEBUG || UNITY_EDITOR
                        BlueprintValidation.ValidateLink(blueprintAsset, nodeMeta, p, linkedNodeMeta, link.portIndex);
#endif

                        var linkedRuntimeNode = GetOrCreateNodeInstance(link.nodeId, linkedNodeMeta);
                        runtimeLinks[l] = new RuntimeLink(linkedRuntimeNode, link.portIndex);
                    }

                    runtimePorts[p] = new RuntimePort(runtimeLinks);
                }

                runtimeNode.RuntimePorts = runtimePorts;
                runtimeNodes[nodeIndex++] = runtimeNode;
            }

            _runtimeNodesMap.Clear();

            return new RuntimeBlueprint(runtimeNodes);
        }

        public RuntimeBlueprint CompileSubgraph(BlueprintAsset blueprintAsset, BlueprintNode subgraph, BlueprintNodeMeta subgraphMeta) {
            var blueprintMeta = blueprintAsset.BlueprintMeta;
            var nodesMetaMap = blueprintMeta.NodesMap;
            int nodesCount = nodesMetaMap.Count;

            int nodeIndex = 0;
            var runtimeNodes = nodesCount > 0
                ? new BlueprintNode[nodesCount]
                : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();

            var externalPortLinksMap = blueprintMeta.ExternalPortLinksMap;
            var subgraphPorts = subgraphMeta.Ports;
            var subgraphRuntimePorts = subgraph.RuntimePorts;
            int subgraphPortsCount = subgraphPorts.Count;

            // Create links from owner subgraph node ports to external nodes inside subgraph, if port is enter or output
            for (int p = 0; p < subgraphPortsCount; p++) {
                var port = subgraphPorts[p];
                if (port.mode is not (Port.Mode.Enter or Port.Mode.Output or Port.Mode.NonTypedOutput)) continue;

                var links = externalPortLinksMap[port.GetSignature()];
                var runtimeLinks = new RuntimeLink[links.Count];

                for (int l = 0; l < links.Count; l++) {
                    var link = links[l];
                    var linkedRuntimeNode = GetOrCreateNodeInstance(link.nodeId, nodesMetaMap[link.nodeId]);

                    runtimeLinks[l] = new RuntimeLink(linkedRuntimeNode, link.portIndex);
                }

                subgraphRuntimePorts[p] = new RuntimePort(runtimeLinks);
            }

            subgraph.RuntimePorts = subgraphRuntimePorts;

            foreach ((int nodeId, var nodeMeta) in nodesMetaMap) {
                var runtimeNode = GetOrCreateNodeInstance(nodeId, nodeMeta);

                var ports = nodeMeta.Ports;
                int portsCount = ports.Count;

                var runtimePorts = portsCount > 0
                    ? new RuntimePort[portsCount]
                    : Array.Empty<RuntimePort>();

                for (int p = 0; p < portsCount; p++) {
#if DEBUG || UNITY_EDITOR
                    BlueprintValidation.ValidatePort(blueprintAsset, nodeMeta, p);
#endif

                    var port = ports[p];

                    // Create link to owner subgraph node, if port is external and exit or input
                    if (port.isExternalPort && port.mode is Port.Mode.Exit or Port.Mode.Output or Port.Mode.NonTypedOutput) {
                        int portSignature = port.GetSignature();
                        int subgraphPortIndex = -1;

                        for (int i = 0; i < subgraphPortsCount; i++) {
                            if (subgraphPorts[i].GetSignature() != portSignature) continue;

                            subgraphPortIndex = i;
                            break;
                        }

                        runtimePorts[p] = new RuntimePort(new[] { new RuntimeLink(subgraph, subgraphPortIndex) });
                        continue;
                    }

                    var links = blueprintMeta.GetLinksFromNodePort(nodeId, p);
                    int linksCount = links.Count;

                    var runtimeLinks = linksCount > 0
                        ? new RuntimeLink[linksCount]
                        : Array.Empty<RuntimeLink>();

                    for (int l = 0; l < linksCount; l++) {
                        var link = links[l];
                        var linkedNodeMeta = nodesMetaMap[link.nodeId];

#if DEBUG || UNITY_EDITOR
                        BlueprintValidation.ValidateLink(blueprintAsset, nodeMeta, p, linkedNodeMeta, link.portIndex);
#endif

                        var linkedRuntimeNode = GetOrCreateNodeInstance(link.nodeId, linkedNodeMeta);
                        runtimeLinks[l] = new RuntimeLink(linkedRuntimeNode, link.portIndex);
                    }

                    runtimePorts[p] = new RuntimePort(runtimeLinks);
                }

                runtimeNode.RuntimePorts = runtimePorts;
                runtimeNodes[nodeIndex++] = runtimeNode;
            }

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
