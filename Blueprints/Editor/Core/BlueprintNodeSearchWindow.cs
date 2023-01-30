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

        public Action<BlueprintNode, Vector2> onNodeCreationRequest = delegate {  };
        public Action<BlueprintNode, Vector2, int> onNodeAndLinkCreationRequest = delegate {  };

        private SearchMode _searchMode;
        private Port _searchPortsCompatibleWith;

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

        public void SwitchToNodePortSearch(Port compatiblePort) {
            _searchMode = SearchMode.NodePorts;
            _searchPortsCompatibleWith = compatiblePort;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            return _searchMode switch {
                SearchMode.Nodes => CreateSearchTreeForNodes(),
                SearchMode.NodePorts => CreateSearchTreeForNodePortsCompatibleWith(_searchPortsCompatibleWith),
                _ => new List<SearchTreeEntry> {
                    new SearchTreeGroupEntry(new GUIContent("Unsupported search mode"), level: 0)
                }
            };
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            switch (searchTreeEntry.userData) {
                case NodeSearchEntry nodeSearchEntry: {
                    var node = Activator.CreateInstance(nodeSearchEntry.nodeType) as BlueprintNode;
                    onNodeCreationRequest.Invoke(node, context.screenMousePosition);
                    return true;
                }
                case NodePortSearchEntry nodePortSearchEntry: {
                    var node = Activator.CreateInstance(nodePortSearchEntry.nodeType) as BlueprintNode;
                    onNodeAndLinkCreationRequest.Invoke(node, context.screenMousePosition, nodePortSearchEntry.portIndex);
                    return true;
                }
                default:
                    return false;
            }
        }

        private static List<SearchTreeEntry> CreateSearchTreeForNodes() {
            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header("Select node") };

            var treeContent = PathTree
                .CreateTree(GetBlueprintNodeSearchEntries(), GetNodePath)
                .PreOrder()
                .RemoveRoot()
                .Select(ToSearchEntry);

            tree.AddRange(treeContent);

            return tree;
        }

        private static List<SearchTreeEntry> CreateSearchTreeForNodePortsCompatibleWith(Port fromPort) {
            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header("Select node and port") };

            var treeContent = PathTree
                .CreateTree(GetBlueprintNodePortsSearchEntries(fromPort), GetNodePortPath)
                .PreOrder()
                .RemoveRoot()
                .Select(ToSearchEntry);

            tree.AddRange(treeContent);

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

        private static SearchTreeEntry ToSearchEntry<T>(TreeEntry<PathTree.Node<T>> treeEntry) {
            return treeEntry.children.Count == 0
                ? SearchTreeEntryUtils.Entry(treeEntry.data.name, treeEntry.data.data, treeEntry.level)
                : SearchTreeEntryUtils.Header(treeEntry.data.name, treeEntry.level);
        }
    }

}
