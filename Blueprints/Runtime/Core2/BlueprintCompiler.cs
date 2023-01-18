using System;
using System.Collections.Generic;

namespace MisterGames.Blueprints.Core2 {

    public sealed class BlueprintCompiler {

        private readonly Dictionary<int, BlueprintNode> _runtimeNodesMap = new Dictionary<int, BlueprintNode>();

        public RuntimeBlueprint Compile(BlueprintMeta blueprintMeta) {
            var nodesMeta = blueprintMeta.Nodes;
            int nodesCount = nodesMeta.Count;

            int nodeIndex = 0;
            var runtimeNodes = nodesCount > 0
                ? new BlueprintNode[nodesCount]
                : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();

            foreach ((int nodeId, var nodeMeta) in nodesMeta) {
                var runtimeNode = GetOrCreateNodeInstance(nodeId, nodeMeta);

                var ports = nodeMeta.Ports;
                int portsCount = ports.Count;

                var runtimePorts = portsCount > 0
                    ? new RuntimePort[portsCount]
                    : Array.Empty<RuntimePort>();

                for (int p = 0; p < portsCount; p++) {
#if DEBUG || UNITY_EDITOR
                    BlueprintValidation.ValidatePort(nodeMeta, p);
#endif

                    var links = blueprintMeta.GetLinksFromNodePort(nodeId, p);
                    int linksCount = links.Count;

                    var runtimeLinks = linksCount > 0
                        ? new RuntimeLink[linksCount]
                        : Array.Empty<RuntimeLink>();

                    for (int l = 0; l < linksCount; l++) {
                        var link = links[l];
                        var linkedNodeMeta = nodesMeta[link.nodeId];

#if DEBUG || UNITY_EDITOR
                        BlueprintValidation.ValidateLink(nodeMeta, p, linkedNodeMeta, link.portIndex);
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

        public RuntimeBlueprint CompileSubgraph(BlueprintMeta blueprintMeta, BlueprintNode subgraph, BlueprintNodeMeta subgraphMeta) {
            var nodesMeta = blueprintMeta.Nodes;
            int nodesCount = nodesMeta.Count;

            int nodeIndex = 0;
            var runtimeNodes = nodesCount > 0
                ? new BlueprintNode[nodesCount]
                : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();

            var subgraphPorts = subgraphMeta.Ports;
            var subgraphRuntimePorts = subgraph.RuntimePorts;

            int subgraphPortsCount = subgraphPorts.Count;

            // Create links from owner subgraph node ports to external nodes, if port is enter or output
            for (int p = 0; p < subgraphPortsCount; p++) {
                var port = subgraphPorts[p];
                if (port.isExitPort && !port.isDataPort || !port.isExitPort && port.isDataPort) continue;

                var link = blueprintMeta.ExternalPortLinks[port.GetHashCode()];
                var linkedRuntimeNode = GetOrCreateNodeInstance(link.nodeId, nodesMeta[link.nodeId]);

                subgraphRuntimePorts[p] = new RuntimePort(new []{ new RuntimeLink(linkedRuntimeNode, link.portIndex) });
            }

            subgraph.RuntimePorts = subgraphRuntimePorts;

            foreach ((int nodeId, var nodeMeta) in nodesMeta) {
                var runtimeNode = GetOrCreateNodeInstance(nodeId, nodeMeta);

                var ports = nodeMeta.Ports;
                int portsCount = ports.Count;

                var runtimePorts = portsCount > 0
                    ? new RuntimePort[portsCount]
                    : Array.Empty<RuntimePort>();

                for (int p = 0; p < portsCount; p++) {
                    var port = ports[p];

                    // Create link to owner subgraph node, if port is external and exit or input
                    if (port.isExternalPort && (port.isExitPort && !port.isDataPort || !port.isExitPort && port.isDataPort)) {
                        int portHashCode = port.GetHashCode();
                        int subgraphPortIndex = -1;

                        for (int i = 0; i < subgraphPortsCount; i++) {
                            if (subgraphPorts[i].GetHashCode() != portHashCode) continue;

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
                        var linkedNodeMeta = nodesMeta[link.nodeId];

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

            nodeInstance = nodeMeta.Node;
            _runtimeNodesMap[nodeId] = nodeInstance;

            if (nodeInstance is IBlueprintCompiledNode compiledNode) compiledNode.Compile(nodeMeta);

            return nodeInstance;
        }
    }

}
