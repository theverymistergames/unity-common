using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Core;
using MisterGames.Blueprints.Meta;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using MisterGames.Blueprints.Validation;
#endif

namespace MisterGames.Blueprints.Compile {

    public sealed class BlueprintCompiler {

        private readonly Dictionary<int, BlueprintNode> _runtimeNodesMap = new Dictionary<int, BlueprintNode>();
        private readonly Dictionary<int, List<BlueprintLink>> _externalPortLinksMap = new Dictionary<int, List<BlueprintLink>>();

        public RuntimeBlueprint Compile(BlueprintAsset blueprint) {
            var nodeIds = blueprint.BlueprintMeta.NodesMap.Keys;
            int nodesCount = nodeIds.Count;
            var runtimeNodes = nodesCount > 0 ? new BlueprintNode[nodesCount] : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();

            int nodeIndex = 0;
            foreach (int nodeId in nodeIds) {
                runtimeNodes[nodeIndex++] = GetOrCompileNode(blueprint, nodeId);
            }

            _runtimeNodesMap.Clear();

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

            if (runtimeNode is IBlueprintCompiledNode compiledNode) compiledNode.Compile(nodeMeta.Ports);

            var ports = nodeMeta.Ports;
            int portsCount = ports.Length;
            runtimeNode.RuntimePorts ??= portsCount > 0 ? new RuntimePort[portsCount] : Array.Empty<RuntimePort>();

            for (int p = 0; p < portsCount; p++) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                PortValidator.ValidatePort(blueprint, nodeMeta, p);
#endif

                var port = ports[p];
                if (port.mode is Port.Mode.Enter or Port.Mode.Output or Port.Mode.NonTypedOutput) continue;

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

                runtimeNode.RuntimePorts[p] = new RuntimePort(runtimeLinks);
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

            int nodeIndex = 0;
            foreach (int nodeId in nodeIds) {
                runtimeNodes[nodeIndex++] = GetOrCompileSubgraphNode(subgraphBlueprint, subgraphNode, subgraphPorts, nodeId);
            }

            int subgraphPortsCount = subgraphPorts.Length;
            subgraphNode.RuntimePorts ??= subgraphPortsCount > 0 ? new RuntimePort[subgraphPortsCount] : Array.Empty<RuntimePort>();

            // Create links from owner subgraph node ports to external nodes inside subgraph, if port is enter or output
            for (int p = 0; p < subgraphPortsCount; p++) {
                var port = subgraphPorts[p];

                if (port.mode is not (Port.Mode.Enter or Port.Mode.Output or Port.Mode.NonTypedOutput)) continue;

                var links = _externalPortLinksMap[port.GetSignature()];
                int linksCount = links.Count;
                var runtimeLinks = new List<RuntimeLink>(linksCount);

                for (int l = 0; l < linksCount; l++) {
                    var link = links[l];
                    var linkedRuntimeNode = GetOrCompileSubgraphNode(subgraphBlueprint, subgraphNode, subgraphPorts, link.nodeId);

                    runtimeLinks.Add(new RuntimeLink(linkedRuntimeNode, link.portIndex));
                }

                subgraphNode.RuntimePorts[p] = new RuntimePort(runtimeLinks);
            }

            _externalPortLinksMap.Clear();
            _runtimeNodesMap.Clear();

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

            if (runtimeNode is IBlueprintCompiledNode compiledNode) compiledNode.Compile(nodeMeta.Ports);

            var ports = nodeMeta.Ports;
            int portsCount = ports.Length;
            runtimeNode.RuntimePorts ??= portsCount > 0 ? new RuntimePort[portsCount] : Array.Empty<RuntimePort>();

            int subgraphPortsCount = subgraphPorts.Length;

            for (int p = 0; p < portsCount; p++) {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                PortValidator.ValidatePort(subgraphBlueprint, nodeMeta, p);
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

                    runtimeNode.RuntimePorts[p] = new RuntimePort(
                        new List<RuntimeLink>(1) { new RuntimeLink(subgraphNode, subgraphPortIndex) }
                    );
                    continue;
                }

                if (port.mode is Port.Mode.Enter or Port.Mode.Output or Port.Mode.NonTypedOutput) continue;

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

                runtimeNode.RuntimePorts[p] = new RuntimePort(runtimeLinks);
            }

            return runtimeNode;
        }

        private void InlineLinks(BlueprintNode node) {
            int nodeHash = node.GetHashCode();
            if (_runtimeNodesMap.ContainsKey(nodeHash)) return;

            _runtimeNodesMap.Add(nodeHash, null);

            var runtimePorts = node.RuntimePorts;

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
                    for (int i = linkedPortIndex; i < count; i++) {
                        var linkedPortLinks = link.node.RuntimePorts[i].links;

                        runtimeLinks.InsertRange(l, linkedPortLinks);
                        l += linkedPortLinks.Count;
                    }

                    runtimeLinks.RemoveAt(l);
                }
            }
        }
    }

}
