using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Editor.Tree;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using NotSupportedException = System.NotSupportedException;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintNodeSearchWindow : ScriptableObject, ISearchWindowProvider {

        public Action<NodeCreationData> onNodeCreationRequest = delegate {  };
        public Action<NodeAndLinkCreationData> onNodeAndLinkCreationRequest = delegate {  };

        private SearchMode _searchMode;
        private PortSearchData _portSearchData;

        public struct PortSearchData {
            public int fromNodeId;
            public int fromPortIndex;
            public Port fromPort;
        }

        public struct NodeCreationData {
            public BlueprintNode node;
            public Vector2 position;
        }

        public struct NodeAndLinkCreationData {
            public BlueprintNode node;
            public Vector2 position;
            public int fromNodeId;
            public int fromPortIndex;
            public int toPortIndex;
        }

        private enum SearchMode {
            Nodes,
            NodePorts,
        }

        private struct NodeSearchEntry {
            public Type nodeType;
        }

        private struct NodePortSearchEntry {
            public Type nodeType;
            public int portIndex;
            public string portName;
        }

        public void SwitchToNodeSearch() {
            _searchMode = SearchMode.Nodes;
        }

        public void SwitchToNodePortSearch(PortSearchData portSearchData) {
            _searchMode = SearchMode.NodePorts;
            _portSearchData = portSearchData;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            return _searchMode switch {
                SearchMode.Nodes => CreateSearchTreeForNodes(),
                SearchMode.NodePorts => CreateSearchTreeForNodePortsCompatibleWith(_portSearchData.fromPort),
                _ => new List<SearchTreeEntry> {
                    new SearchTreeGroupEntry(new GUIContent("Unsupported search mode"), level: 0)
                }
            };
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            switch (searchTreeEntry.userData) {
                case NodeSearchEntry nodeSearchEntry: {
                    var nodeCreationData = new NodeCreationData {
                        node = Activator.CreateInstance(nodeSearchEntry.nodeType) as BlueprintNode,
                        position = context.screenMousePosition,
                    };
                    onNodeCreationRequest.Invoke(nodeCreationData);
                    return true;
                }

                case NodePortSearchEntry nodePortSearchEntry: {
                    var nodeAndLinkCreationData = new NodeAndLinkCreationData {
                        node = Activator.CreateInstance(nodePortSearchEntry.nodeType) as BlueprintNode,
                        position = context.screenMousePosition,
                        fromNodeId = _portSearchData.fromNodeId,
                        fromPortIndex = _portSearchData.fromPortIndex,
                        toPortIndex = nodePortSearchEntry.portIndex,
                    };
                    onNodeAndLinkCreationRequest.Invoke(nodeAndLinkCreationData);
                    return true;
                }

                default:
                    return false;
            }
        }

        private static List<SearchTreeEntry> CreateSearchTreeForNodes() {
            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header("Select node") };

            tree.AddRange(PathTree
                .CreateTree(GetBlueprintNodeSearchEntries(), GetNodePath)
                .Select(ToSearchEntry)
            );

            return tree;
        }

        private static List<SearchTreeEntry> CreateSearchTreeForNodePortsCompatibleWith(Port fromPort) {
            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header("Select node and port") };

            tree.AddRange(PathTree
                .CreateTree(GetBlueprintNodePortsSearchEntries(fromPort), GetNodePortPath)
                .Select(ToSearchEntry)
            );

            return tree;
        }

        private static IEnumerable<NodeSearchEntry> GetBlueprintNodeSearchEntries() {
            return GetBlueprintNodeTypes().Select(t => new NodeSearchEntry { nodeType = t });
        }

        private static IEnumerable<NodePortSearchEntry> GetBlueprintNodePortsSearchEntries(Port fromPort) {
            return GetBlueprintNodeTypes()
                .SelectMany(t => {
                    var nodeInstance = (BlueprintNode) Activator.CreateInstance(t);
                    var ports = nodeInstance.CreatePorts();
                    var portEntries = new List<NodePortSearchEntry>();

                    for (int p = 0; p < ports.Length; p++) {
                        var port = ports[p];
                        if (!BlueprintValidation.ArePortsCompatible(fromPort, port)) continue;

                        portEntries.Add(new NodePortSearchEntry {
                            nodeType = t,
                            portIndex = p,
                            portName = GetPortName(p, port),
                        });
                    }

                    return portEntries;
                });
        }

        private static string GetNodePath(NodeSearchEntry nodeSearchEntry) {
            return GetNodeTypePath(nodeSearchEntry.nodeType);
        }

        private static string GetNodePortPath(NodePortSearchEntry nodePortSearchEntry) {
            return $"{GetNodeTypePath(nodePortSearchEntry.nodeType)}: {nodePortSearchEntry.portName}";
        }

        private static string GetPortName(int index, Port port) {
            return string.IsNullOrEmpty(port.name)
                ? port.mode switch {
                    Port.Mode.Enter => $"[{index}] Enter",
                    Port.Mode.Exit => $"[{index}] Exit",
                    Port.Mode.Input => $"[{index}] In ({port.DataType.Name})",
                    Port.Mode.Output => $"[{index}] Out ({port.DataType.Name})",
                    Port.Mode.NonTypedInput => $"[{index}] In",
                    Port.Mode.NonTypedOutput => $"[{index}] Out",
                    _ => throw new NotSupportedException($"Port mode {port.mode} is not supported")
                }
                : port.mode switch {
                    Port.Mode.Enter => $"[{index}] {port.name}",
                    Port.Mode.Exit => $"[{index}] {port.name}",
                    Port.Mode.Input => $"[{index}] {port.name} ({port.DataType.Name})",
                    Port.Mode.Output => $"[{index}] {port.name} ({port.DataType.Name})",
                    Port.Mode.NonTypedInput => $"[{index}] {port.name}",
                    Port.Mode.NonTypedOutput => $"[{index}] {port.name}",
                    _ => throw new NotSupportedException($"Port mode {port.mode} is not supported")
                };
        }

        private static IEnumerable<Type> GetBlueprintNodeTypes() {
            return TypeCache
                .GetTypesDerivedFrom<BlueprintNode>()
                .Where(t => !t.IsAbstract && t.IsVisible && HasBlueprintNodeMetaAttribute(t) && HasSerializableAttribute(t));
        }

        private static string GetNodeTypePath(Type nodeType) {
            var nodeMetaAttr = GetBlueprintNodeMetaAttribute(nodeType);

            string name = string.IsNullOrEmpty(nodeMetaAttr.Name) ? nodeType.Name : nodeMetaAttr.Name;
            string category = string.IsNullOrEmpty(nodeMetaAttr.Category) ? "Other" : nodeMetaAttr.Category;

            return $"{category}/{name}";
        }

        private static bool HasBlueprintNodeMetaAttribute(Type type) {
            return GetBlueprintNodeMetaAttribute(type) != null;
        }

        private static bool HasSerializableAttribute(Type type) {
            return type.GetCustomAttribute<SerializableAttribute>(false) != null;
        }

        private static BlueprintNodeMetaAttribute GetBlueprintNodeMetaAttribute(Type type) {
            return type.GetCustomAttribute<BlueprintNodeMetaAttribute>(false);
        }

        private static SearchTreeEntry ToSearchEntry<T>(PathTree.Node<T> node) {
            return node.isLeaf
                ? SearchTreeEntryUtils.Entry(node.name, node.data, node.level)
                : SearchTreeEntryUtils.Header(node.name, node.level);
        }
    }

}
