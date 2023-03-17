using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;
using UnityEngine;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using MisterGames.Blueprints.Validation;
#endif

namespace MisterGames.Blueprints.Compile {

    public sealed class BlueprintCompiler {

        private readonly Dictionary<int, BlueprintNode> _runtimeNodesMap = new Dictionary<int, BlueprintNode>();
        private readonly Dictionary<int, List<BlueprintLink>> _externalPortLinksMap = new Dictionary<int, List<BlueprintLink>>();
        private readonly Dictionary<int, List<BlueprintLink>> _nodeLinkerGroupsMap = new Dictionary<int, List<BlueprintLink>>();

        public RuntimeBlueprint Compile(BlueprintAsset blueprint) {
            var nodeIds = blueprint.BlueprintMeta.NodesMap.Keys;
            int nodesCount = nodeIds.Count;
            var runtimeNodes = nodesCount > 0 ? new BlueprintNode[nodesCount] : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();
            _nodeLinkerGroupsMap.Clear();

            int nodeIndex = 0;
            foreach (int nodeId in nodeIds) {
                runtimeNodes[nodeIndex++] = GetOrCompileNode(blueprint, nodeId);
            }

            CreateConnectionsBetweenNodeLinkers(blueprint);

            _runtimeNodesMap.Clear();
            _nodeLinkerGroupsMap.Clear();

            for (int n = 0; n < runtimeNodes.Length; n++) {
                InlineLinks(runtimeNodes[n]);
            }

            _runtimeNodesMap.Clear();

            return new RuntimeBlueprint(runtimeNodes);
        }

        private BlueprintNode GetOrCompileNode(BlueprintAsset blueprint, int nodeId) {
            if (_runtimeNodesMap.TryGetValue(nodeId, out var runtimeNode)) return runtimeNode;

            var blueprintMeta = blueprint.BlueprintMeta;
            var nodesMetaMap = blueprintMeta.NodesMap;
            var nodeMeta = nodesMetaMap[nodeId];

            runtimeNode = nodeMeta.CreateNodeInstance();
            _runtimeNodesMap[nodeId] = runtimeNode;

            if (runtimeNode is IBlueprintNodeLinker linker) AddNodeLinker(nodeId, linker);
            if (runtimeNode is IBlueprintCompiledNode compiledNode) compiledNode.Compile(nodeMeta.Ports);

            var ports = nodeMeta.Ports;
            int portsCount = ports.Length;
            runtimeNode.Ports ??= portsCount > 0 ? new RuntimePort[portsCount] : Array.Empty<RuntimePort>();

            for (int p = 0; p < portsCount; p++) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                PortValidator.ValidatePort(blueprint, nodeMeta, p);
#endif

                var port = ports[p];

                // Skip enter or data-based output ports
                if (port.IsInput != port.IsData) continue;

                var links = blueprintMeta.GetLinksFromNodePort(nodeId, p);
                int linksCount = links.Count;
                var runtimeLinks = new List<RuntimeLink>(linksCount);

                for (int l = 0; l < linksCount; l++) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    LinkValidator.ValidateLink(blueprint, nodeMeta, p, nodesMetaMap[links[l].nodeId], links[l].portIndex);
#endif

                    var link = links[l];
                    runtimeLinks.Add(new RuntimeLink(GetOrCompileNode(blueprint, link.nodeId), link.portIndex));
                }

                runtimeNode.Ports[p] = new RuntimePort(runtimeLinks);
            }

            return runtimeNode;
        }

        public RuntimeBlueprint CompileSubgraph(
            BlueprintAsset subgraphBlueprint,
            BlueprintNode subgraphNode,
            Port[] subgraphPorts
        ) {
            var blueprintMeta = subgraphBlueprint.BlueprintMeta;
            var nodeIds = blueprintMeta.NodesMap.Keys;

            int nodesCount = nodeIds.Count;
            var runtimeNodes = nodesCount > 0 ? new BlueprintNode[nodesCount] : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();
            _externalPortLinksMap.Clear();
            _nodeLinkerGroupsMap.Clear();

            int nodeIndex = 0;
            foreach (int nodeId in nodeIds) {
                runtimeNodes[nodeIndex++] = GetOrCompileSubgraphNode(subgraphBlueprint, subgraphNode, subgraphPorts, nodeId);
            }

            int subgraphPortsCount = subgraphPorts.Length;
            subgraphNode.Ports ??= subgraphPortsCount > 0 ? new RuntimePort[subgraphPortsCount] : Array.Empty<RuntimePort>();

            // Create links from owner subgraph node ports to external nodes inside subgraph
            for (int p = 0; p < subgraphPortsCount; p++) {
                var port = subgraphPorts[p];

                // Skip exit or data-based input ports
                if (port.IsInput == port.IsData) continue;

                var links = _externalPortLinksMap[port.GetSignatureHashCode()];
                int linksCount = links.Count;
                var runtimeLinks = new List<RuntimeLink>(linksCount);

                for (int l = 0; l < linksCount; l++) {
                    var link = links[l];
                    var linkedRuntimeNode = GetOrCompileSubgraphNode(subgraphBlueprint, subgraphNode, subgraphPorts, link.nodeId);

                    runtimeLinks.Add(new RuntimeLink(linkedRuntimeNode, link.portIndex));
                }

                subgraphNode.Ports[p] = new RuntimePort(runtimeLinks);
            }

            CreateConnectionsBetweenNodeLinkers(subgraphBlueprint);

            _externalPortLinksMap.Clear();
            _runtimeNodesMap.Clear();
            _nodeLinkerGroupsMap.Clear();

            return new RuntimeBlueprint(runtimeNodes);
        }

        private BlueprintNode GetOrCompileSubgraphNode(
            BlueprintAsset subgraphBlueprint,
            BlueprintNode subgraphNode,
            Port[] subgraphPorts,
            int nodeId
        ) {
            if (_runtimeNodesMap.TryGetValue(nodeId, out var runtimeNode)) return runtimeNode;

            var blueprintMeta = subgraphBlueprint.BlueprintMeta;
            var nodesMetaMap = blueprintMeta.NodesMap;
            var nodeMeta = nodesMetaMap[nodeId];

            runtimeNode = nodeMeta.CreateNodeInstance();
            _runtimeNodesMap[nodeId] = runtimeNode;

            if (runtimeNode is IBlueprintNodeLinker linker) AddNodeLinker(nodeId, linker);
            if (runtimeNode is IBlueprintCompiledNode compiledNode) compiledNode.Compile(nodeMeta.Ports);

            var ports = nodeMeta.Ports;
            int portsCount = ports.Length;
            runtimeNode.Ports ??= portsCount > 0 ? new RuntimePort[portsCount] : Array.Empty<RuntimePort>();

            int subgraphPortsCount = subgraphPorts.Length;

            for (int p = 0; p < portsCount; p++) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                PortValidator.ValidatePort(subgraphBlueprint, nodeMeta, p);
#endif

                var port = ports[p];
                if (port.IsExternal) {
                    int portSignature = port.GetSignatureHashCode();

                    // External port is enter or output: add port address as link into external ports map
                    // to create links from matched subgraph node port to this external port.
                    if (port.IsInput != port.IsData) {
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
                        if (subgraphPorts[i].GetSignatureHashCode() != portSignature) continue;

                        subgraphPortIndex = i;
                        break;
                    }

                    runtimeNode.Ports[p] = new RuntimePort(
                        new List<RuntimeLink>(1) { new RuntimeLink(subgraphNode, subgraphPortIndex) }
                    );
                    continue;
                }

                // Skip enter or data-based output ports
                if (port.IsInput != port.IsData) continue;

                var links = blueprintMeta.GetLinksFromNodePort(nodeId, p);
                int linksCount = links.Count;
                var runtimeLinks = new List<RuntimeLink>(linksCount);

                for (int l = 0; l < linksCount; l++) {
                    var link = links[l];

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    LinkValidator.ValidateLink(subgraphBlueprint, nodeMeta, p, nodesMetaMap[link.nodeId], link.portIndex);
#endif

                    var linkedNode = GetOrCompileSubgraphNode(subgraphBlueprint, subgraphNode, subgraphPorts, link.nodeId);
                    runtimeLinks.Add(new RuntimeLink(linkedNode, link.portIndex));
                }

                runtimeNode.Ports[p] = new RuntimePort(runtimeLinks);
            }

            return runtimeNode;
        }

        private void AddNodeLinker(int nodeId, IBlueprintNodeLinker linker) {
            int hash = linker.LinkerNodeHash;
            var link = new BlueprintLink { nodeId = nodeId, portIndex = linker.LinkerNodePort };

            if (_nodeLinkerGroupsMap.TryGetValue(linker.LinkerNodeHash, out var links)) links.Add(link);
            else _nodeLinkerGroupsMap.Add(hash, new List<BlueprintLink> { link });
        }

        private void CreateConnectionsBetweenNodeLinkers(BlueprintAsset blueprint) {
            var nodesMap = blueprint.BlueprintMeta.NodesMap;
            var nodeLinkerGroups = _nodeLinkerGroupsMap.Values;

            if (nodeLinkerGroups.Count == 0) return;

            foreach (var group in nodeLinkerGroups) {
                if (group == null || group.Count == 0) continue;

                var runtimeLinks = new List<RuntimeLink>(group.Count);

                for (int i = 0; i < group.Count; i++) {
                    var link = group[i];
                    if (!nodesMap.TryGetValue(link.nodeId, out var nodeMeta)) continue;

                    var ports = nodeMeta.Ports;
                    if (link.portIndex < 0 || link.portIndex > ports.Length - 1) continue;

                    var port = ports[link.portIndex];
                    var node = _runtimeNodesMap[link.nodeId];

                    // Exit or data-based input port: link owners
                    if (port.IsInput == port.IsData) {
                        node.Ports ??= new RuntimePort[ports.Length];
                        node.Ports[link.portIndex] = new RuntimePort(runtimeLinks);
                        continue;
                    }

                    // Enter or data-based output port: link targets
                    runtimeLinks.Add(new RuntimeLink(node, link.portIndex));
                }
            }
        }

        private void InlineLinks(BlueprintNode node) {
            int nodeHash = node.GetHashCode();
            if (_runtimeNodesMap.ContainsKey(nodeHash)) return;

            _runtimeNodesMap.Add(nodeHash, null);

            var runtimePorts = node.Ports;

            for (int p = 0; p < runtimePorts.Length; p++) {
                var runtimePort = runtimePorts[p];
                var runtimeLinks = runtimePort.links;

                if (runtimeLinks == null) continue;

                for (int l = runtimeLinks.Count - 1; l >= 0; l--) {
                    var link = runtimeLinks[l];

                    if (link.node is not IBlueprintPortLinker linker) {
                        InlineLinks(link.node);
                        continue;
                    }

                    int linkedPortIndex = linker.GetLinkedPorts(link.port, out int count);
                    var linkedNodePorts = link.node.Ports;

                    for (int i = 0; i < count; i++) {
                        var linkedPortLinks = linkedNodePorts[linkedPortIndex + i].links;
                        runtimeLinks.InsertRange(l, linkedPortLinks);
                        l += linkedPortLinks.Count;
                    }

                    runtimeLinks.RemoveAt(l);
                }
            }
        }
    }

}
