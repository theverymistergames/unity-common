using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Utils2;

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

                var ports = nodeMeta.ports;
                int portsLength = ports.Length;

                var portCompileDataArray = portsLength > 0
                    ? new BlueprintPortCompileData[portsLength]
                    : Array.Empty<BlueprintPortCompileData>();

                for (int p = 0; p < portsLength; p++) {
#if DEBUG
                    if (!BlueprintValidationUtils.ValidatePort(nodeMeta, p)) continue;
#endif
                    int linksCount = meta.GetNodePortConnectionsCount(nodeId, p);
                    var linkCompileDataArray = linksCount > 0
                        ? new BlueprintLinkCompileData[linksCount]
                        : Array.Empty<BlueprintLinkCompileData>();

                    for (int l = 0; l < linksCount; l++) {
                        int connectionId = meta.GetNodePortConnectionId(nodeId, p, l);
                        var connection = connections[connectionId];
#if DEBUG
                        if (!BlueprintValidationUtils.ValidateLink(nodeMeta, p, nodes[connection.toNodeId], connection.toPortIndex)) continue;
#endif
                        linkCompileDataArray[l] = new BlueprintLinkCompileData(connection.toNodeId, connection.toPortIndex);
                    }

                    portCompileDataArray[p] = new BlueprintPortCompileData(linkCompileDataArray);
                }

                _nodeCompileDataMap[nodeId] = new BlueprintNodeCompileData(nodeMeta, portCompileDataArray);
            }
        }

        public void Clear() {
            _nodesCount = 0;

            _nodeCompileDataMap?.Clear();
            _nodeIds?.Clear();

            _runtimeNodesMap.Clear();
        }

        public RuntimeBlueprint Compile() {
            var runtimeNodesArray = _nodesCount > 0 ? new BlueprintNode[_nodesCount] : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();

            for (int i = 0; i < _nodesCount; i++) {
                int nodeId = _nodeIds[i];
                var nodeCompileData = _nodeCompileDataMap[nodeId];

                // Get or create current node instance
                if (!_runtimeNodesMap.TryGetValue(nodeId, out var runtimeNode)) {
                    runtimeNode = CreateBlueprintNodeInstance(nodeCompileData.nodeMeta);
                    _runtimeNodesMap[nodeId] = runtimeNode;
                }

                runtimeNode.InjectRuntimePorts(CompileNodePorts(nodeCompileData.ports));
                runtimeNodesArray[i] = runtimeNode;
            }

            _runtimeNodesMap.Clear();

            return new RuntimeBlueprint(runtimeNodesArray);
        }

        private RuntimePort[] CompileNodePorts(BlueprintPortCompileData[] ports) {
            var runtimePorts = ports.Length > 0 ? new RuntimePort[ports.Length] : Array.Empty<RuntimePort>();

            for (int p = 0; p < ports.Length; p++) {
                runtimePorts[p] = new RuntimePort(CompilePortLinks(ports[p].links));
            }

            return runtimePorts;
        }

        private RuntimeLink[] CompilePortLinks(BlueprintLinkCompileData[] links) {
            var runtimeLinks = links.Length > 0 ? new RuntimeLink[links.Length] : Array.Empty<RuntimeLink>();

            for (int l = 0; l < links.Length; l++) {
                var link = links[l];

                if (!_runtimeNodesMap.TryGetValue(link.nodeId, out var runtimeNode)) {
                    runtimeNode = CreateBlueprintNodeInstance(_nodeCompileDataMap[link.nodeId].nodeMeta);
                    _runtimeNodesMap[link.nodeId] = runtimeNode;
                }

                runtimeLinks[l] = new RuntimeLink(runtimeNode, link.portIndex);
            }

            return runtimeLinks;
        }

        private BlueprintNode CreateBlueprintNodeInstance(BlueprintNodeMeta nodeMeta) {
            return (BlueprintNode) Activator.CreateInstance(nodeMeta.node.GetType());
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
