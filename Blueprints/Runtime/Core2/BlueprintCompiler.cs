using System;
using System.Collections.Generic;

namespace MisterGames.Blueprints.Core2 {

    public sealed class BlueprintCompiler {

        private readonly Dictionary<int, BlueprintNode> _runtimeNodesMap = new Dictionary<int, BlueprintNode>();

        private int _nodesCount;
        private List<int> _nodeIds;
        private Dictionary<int, BlueprintNodeCompileData> _nodeCompileDataMap;

        public void Prepare(BlueprintMeta meta) {
            Clear();

            var nodes = meta.Nodes;
            var connections = meta.Connections;

            _nodesCount = nodes.Count;
            _nodeIds = new List<int>(nodes.Keys);
            _nodeCompileDataMap = new Dictionary<int, BlueprintNodeCompileData>(_nodesCount);

            for (int n = 0; n < nodes.Count; n++) {
                int nodeId = _nodeIds[n];
                var nodeMeta = nodes[nodeId];

                int portsCount = nodeMeta.Ports.Count;
                var ports = portsCount > 0
                    ? new BlueprintPortCompileData[portsCount]
                    : Array.Empty<BlueprintPortCompileData>();

                for (int p = 0; p < portsCount; p++) {
#if DEBUG || UNITY_EDITOR
                    if (!BlueprintValidationUtils.ValidatePort(nodeMeta, p)) continue;
#endif

                    int linksCount = meta.GetNodePortConnectionsCount(nodeId, p);
                    var links = linksCount > 0
                        ? new BlueprintLinkCompileData[linksCount]
                        : Array.Empty<BlueprintLinkCompileData>();

                    for (int l = 0; l < linksCount; l++) {
                        int connectionId = meta.GetNodePortConnectionId(nodeId, p, l);
                        var connection = connections[connectionId];

#if DEBUG || UNITY_EDITOR
                        if (!BlueprintValidationUtils.ValidateLink(nodeMeta, p, nodes[connection.toNodeId], connection.toPortIndex)) continue;
#endif

                        links[l] = new BlueprintLinkCompileData(connection.toNodeId, connection.toPortIndex);
                    }

                    ports[p] = new BlueprintPortCompileData(links);
                }

                _nodeCompileDataMap[nodeId] = new BlueprintNodeCompileData(nodeMeta, ports);
            }
        }

        public RuntimeBlueprint Compile() {
            var runtimeNodes = _nodesCount > 0
                ? new BlueprintNode[_nodesCount]
                : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();

            for (int i = 0; i < _nodesCount; i++) {
                int nodeId = _nodeIds[i];
                var nodeCompileData = _nodeCompileDataMap[nodeId];

                if (!_runtimeNodesMap.TryGetValue(nodeId, out var runtimeNode)) {
                    runtimeNode = CreateBlueprintNodeInstance(nodeCompileData.nodeMeta);
                    _runtimeNodesMap[nodeId] = runtimeNode;
                }

                var ports = nodeCompileData.ports;
                int portsCount = ports.Length;

                var runtimePorts = portsCount > 0
                    ? new RuntimePort[portsCount]
                    : Array.Empty<RuntimePort>();

                for (int p = 0; p < portsCount; p++) {
                    var links = ports[p].links;
                    int linksCount = links.Length;

                    var runtimeLinks = linksCount > 0
                        ? new RuntimeLink[linksCount]
                        : Array.Empty<RuntimeLink>();

                    for (int l = 0; l < links.Length; l++) {
                        var link = links[l];

                        if (!_runtimeNodesMap.TryGetValue(link.nodeId, out var linkedRuntimeNode)) {
                            var linkedNodeMeta = _nodeCompileDataMap[link.nodeId].nodeMeta;
                            linkedRuntimeNode = CreateBlueprintNodeInstance(linkedNodeMeta);
                            _runtimeNodesMap[link.nodeId] = linkedRuntimeNode;
                        }

                        runtimeLinks[l] = new RuntimeLink(linkedRuntimeNode, link.portIndex);
                    }

                    runtimePorts[p] = new RuntimePort(runtimeLinks);
                }

                runtimeNode.InjectRuntimePorts(runtimePorts);
                runtimeNodes[i] = runtimeNode;
            }

            _runtimeNodesMap.Clear();

            return new RuntimeBlueprint(runtimeNodes);
        }

        public void Clear() {
            _nodesCount = 0;

            _nodeCompileDataMap?.Clear();
            _nodeIds?.Clear();

            _runtimeNodesMap.Clear();
        }

        private static BlueprintNode CreateBlueprintNodeInstance(BlueprintNodeMeta nodeMeta) {
            return (BlueprintNode) Activator.CreateInstance(nodeMeta.NodeType);
        }

        private readonly struct BlueprintNodeCompileData {

            public readonly BlueprintNodeMeta nodeMeta;
            public readonly BlueprintPortCompileData[] ports;

            public BlueprintNodeCompileData(BlueprintNodeMeta nodeMeta, BlueprintPortCompileData[] ports) {
                this.nodeMeta = nodeMeta;
                this.ports = ports;
            }
        }

        private readonly struct BlueprintPortCompileData {

            public readonly BlueprintLinkCompileData[] links;

            public BlueprintPortCompileData(BlueprintLinkCompileData[] links) {
                this.links = links;
            }
        }

        private readonly struct BlueprintLinkCompileData {

            public readonly int nodeId;
            public readonly int portIndex;

            public BlueprintLinkCompileData(int nodeId, int portIndex) {
                this.nodeId = nodeId;
                this.portIndex = portIndex;
            }
        }
    }

}
