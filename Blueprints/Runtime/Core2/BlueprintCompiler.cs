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
                    if (!BlueprintValidation.ValidatePort(nodeMeta, p)) continue;
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
                        if (!BlueprintValidation.ValidateLink(nodeMeta, p, linkedNodeMeta, link.portIndex)) continue;
#endif
                        var linkedRuntimeNode = GetOrCreateNodeInstance(link.nodeId, linkedNodeMeta);

                        runtimeLinks[l] = new RuntimeLink(linkedRuntimeNode, link.portIndex);
                    }

                    runtimePorts[p] = new RuntimePort(runtimeLinks);
                }

                runtimeNode.InjectRuntimePorts(runtimePorts);
                runtimeNodes[nodeIndex++] = runtimeNode;
            }

            _runtimeNodesMap.Clear();

            return new RuntimeBlueprint(runtimeNodes);
        }

        private BlueprintNode GetOrCreateNodeInstance(int nodeId, BlueprintNodeMeta nodeMeta) {
            if (_runtimeNodesMap.TryGetValue(nodeId, out var nodeInstance)) return nodeInstance;

            nodeInstance = (BlueprintNode) Activator.CreateInstance(nodeMeta.Node.GetType());
            _runtimeNodesMap[nodeId] = nodeInstance;

            return nodeInstance;
        }
    }

}
