using System;
using System.Collections.Generic;
using System.Text;
using MisterGames.Blueprints.Meta;
using MisterGames.Blueprints.Validation;

namespace MisterGames.Blueprints.Compile {

    public sealed class BlueprintCompiler {

        private readonly Dictionary<int, BlueprintNode> _runtimeNodesMap = new Dictionary<int, BlueprintNode>();
        private readonly Dictionary<int, List<BlueprintLink>> _externalPortLinksMap = new Dictionary<int, List<BlueprintLink>>();

        public RuntimeBlueprint Compile(BlueprintAsset blueprint) {
            var blueprintMeta = blueprint.BlueprintMeta;
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
                    PortValidator.ValidatePort(blueprint, nodeMeta, p);
#endif

                    var port = ports[p];
                    if (port.mode is Port.Mode.Enter or Port.Mode.Output or Port.Mode.NonTypedOutput) continue;

                    var links = blueprintMeta.GetLinksFromNodePort(nodeId, p);
                    int linksCount = links.Count;
                    var runtimeLinks = new List<RuntimeLink>(linksCount);

                    for (int l = 0; l < linksCount; l++) {
                        var link = links[l];

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                        LinkValidator.ValidateLink(blueprint, nodeMeta, p, nodesMetaMap[link.nodeId], link.portIndex);
#endif

                        runtimeLinks.Add(new RuntimeLink(GetOrCreateNodeInstance(link.nodeId, nodesMetaMap[link.nodeId]), link.portIndex));
                    }

                    runtimeNode.RuntimePorts[p] = new RuntimePort(runtimeLinks);
                }

                runtimeNodes[nodeIndex++] = runtimeNode;
            }

            _runtimeNodesMap.Clear();

            for (int n = 0; n < runtimeNodes.Length; n++) {
                InlineLinks(runtimeNodes[n]);
            }

            return new RuntimeBlueprint(runtimeNodes);
        }

        public RuntimeBlueprint CompileSubgraph(
            BlueprintAsset subgraphBlueprint,
            BlueprintNode subgraphNode,
            BlueprintNodeMeta subgraphNodeMeta
        ) {
            var blueprintMeta = subgraphBlueprint.BlueprintMeta;
            var nodesMetaMap = blueprintMeta.NodesMap;

            int nodesCount = nodesMetaMap.Count;
            int nodeIndex = 0;
            var runtimeNodes = nodesCount > 0 ? new BlueprintNode[nodesCount] : Array.Empty<BlueprintNode>();

            _runtimeNodesMap.Clear();
            _externalPortLinksMap.Clear();

            var subgraphPorts = subgraphNodeMeta.Ports;
            int subgraphPortsCount = subgraphPorts.Length;

            foreach ((int nodeId, var nodeMeta) in nodesMetaMap) {
                var runtimeNode = GetOrCreateNodeInstance(nodeId, nodeMeta);

                var ports = nodeMeta.Ports;
                int portsCount = ports.Length;
                runtimeNode.RuntimePorts ??= portsCount > 0 ? new RuntimePort[portsCount] : Array.Empty<RuntimePort>();

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

                        runtimeNode.RuntimePorts[p] = new RuntimePort(new List<RuntimeLink>(1) { new RuntimeLink(subgraphNode, subgraphPortIndex) });
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

                        runtimeLinks.Add(new RuntimeLink(GetOrCreateNodeInstance(link.nodeId, nodesMetaMap[link.nodeId]), link.portIndex));
                    }

                    runtimeNode.RuntimePorts[p] = new RuntimePort(runtimeLinks);
                }

                runtimeNodes[nodeIndex++] = runtimeNode;
            }

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
                    runtimeLinks.Add(new RuntimeLink(GetOrCreateNodeInstance(link.nodeId, nodesMetaMap[link.nodeId]), link.portIndex));
                }

                subgraphNode.RuntimePorts[p] = new RuntimePort(runtimeLinks);
            }

            _externalPortLinksMap.Clear();
            _runtimeNodesMap.Clear();

            return new RuntimeBlueprint(runtimeNodes);
        }

        private static void InlineLinks(BlueprintNode node) {
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

                    int linkedPortIndex = linker.GetLinkedPort(link.port);
                    var linkedPortLinks = link.node.RuntimePorts[linkedPortIndex].links;
                    runtimeLinks.InsertRange(l, linkedPortLinks);

                    l += linkedPortLinks.Count;
                    runtimeLinks.RemoveAt(l);
                }
            }
        }

        private BlueprintNode GetOrCreateNodeInstance(int nodeId, BlueprintNodeMeta nodeMeta) {
            if (_runtimeNodesMap.TryGetValue(nodeId, out var nodeInstance)) return nodeInstance;

            nodeInstance = nodeMeta.CreateNodeInstance();
            _runtimeNodesMap[nodeId] = nodeInstance;

            if (nodeInstance is IBlueprintCompiledNode compiledNode) compiledNode.Compile(nodeMeta);

            return nodeInstance;
        }

        private static string RuntimeNodesToString(string prefix, BlueprintNode[] nodes) {
            var sb = new StringBuilder();

            sb.AppendLine(prefix);

            for (int n = 0; n < nodes.Length; n++) {
                var node = nodes[n];

                sb.AppendLine(RuntimeNodeToString(node));
            }

            return sb.ToString();
        }

        private static string RuntimeNodeToString(BlueprintNode node) {
            var sb = new StringBuilder();

            sb.AppendLine($"-- Node {node} (hash {node.GetHashCode()})");

            var ports = node.RuntimePorts;

            for (int p = 0; p < ports.Length; p++) {
                var port = ports[p];
                var links = port.links;

                if (links == null) {
                    sb.AppendLine($"---- Port#{p}: links: <null>");
                    continue;
                }

                if (links.Count == 0) {
                    sb.AppendLine($"---- Port#{p}: links: <empty>");
                    continue;
                }

                sb.AppendLine($"---- Port#{p}: links:");

                for (int l = 0; l < links.Count; l++) {
                    var link = links[l];

                    sb.AppendLine($"-------- Link#{l}: {link.node} (hash {link.node.GetHashCode()}) :: {link.port}");
                }
            }

            return sb.ToString();
        }
    }

}
