using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Blueprints.Editor.Utils;
using MisterGames.Blueprints.Editor.View;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Validation;
using MisterGames.Common.Editor.Tree;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Windows {

    public sealed class BlueprintNodeSearchWindow : ScriptableObject, ISearchWindowProvider {

        public Action<Type, Vector2> onNodeCreationRequest = delegate {  };
        public Action<Type, Vector2, int> onNodeAndLinkCreationRequest = delegate {  };

        private SearchMode _searchMode;
        private Port _searchPortsCompatibleWith;
        private readonly BlueprintNodePortCache _portCache = new BlueprintNodePortCache();

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

            public override string ToString() {
                return $"NodePortSearchEntry(nodeType {nodeType}, port#{portIndex} {portName})";
            }
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
            var position = context.screenMousePosition;

            switch (searchTreeEntry.userData) {
                case NodeSearchEntry nodeSearchEntry: {
                    onNodeCreationRequest.Invoke(nodeSearchEntry.nodeType, position);
                    return true;
                }

                case NodePortSearchEntry nodePortSearchEntry: {
                    onNodeAndLinkCreationRequest.Invoke(nodePortSearchEntry.nodeType, position, nodePortSearchEntry.portIndex);
                    return true;
                }

                default:
                    return false;
            }
        }

        private static List<SearchTreeEntry> CreateSearchTreeForNodes() {
            var root = PathTree
                .CreateTree(GetBlueprintNodeSearchEntries(), GetNodePath);

            var treeContent = root
                .PreOrder()
                .Where(e => e.level > 0)
                .Select(ToSearchEntry);

            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header($"Select node {root.data.name}") };

            tree.AddRange(treeContent);

            return tree;
        }

        private List<SearchTreeEntry> CreateSearchTreeForNodePortsCompatibleWith(Port fromPort) {
            var root = PathTree
                .CreateTree(GetBlueprintNodePortsSearchEntries(fromPort), GetNodePortPath);

             var treeContent = root
                .PreOrder()
                .Where(e => e.level > 0)
                .Select(ToSearchEntry);

            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header($"Select node and port {root.data.name}") };

            tree.AddRange(treeContent);

            return tree;
        }

        private static IEnumerable<NodeSearchEntry> GetBlueprintNodeSearchEntries() {
            return GetBlueprintNodeTypes().Select(t => new NodeSearchEntry { nodeType = t });
        }

        private IEnumerable<NodePortSearchEntry> GetBlueprintNodePortsSearchEntries(Port fromPort) {
            return GetBlueprintNodeTypes()
                .SelectMany(t => {
                    var sourceType = BlueprintNodeUtils.GetSourceType(t);
                    var source = Activator.CreateInstance(sourceType) as IBlueprintSource;

                    var portEntries = new List<NodePortSearchEntry>();
                    if (source == null) return portEntries;

                    int nodeId = source.AddNode(t);
                    var id = new NodeId(0, nodeId);

                    _portCache.Clear();

                    source.OnSetDefaults(_portCache, id);
                    source.CreatePorts(_portCache, id);

                    int portCount = _portCache.GetPortCount(id);
                    for (int p = 0; p < portCount; p++) {
                        var port = _portCache.GetPort(id, p);
                        if (!PortValidator.ArePortsCompatible(fromPort, port)) continue;

                        portEntries.Add(new NodePortSearchEntry {
                            nodeType = t,
                            portIndex = p,
                            portName = BlueprintNodeMetaUtils.GetFormattedPortName(p, port, richText: false),
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

        private static IEnumerable<Type> GetBlueprintNodeTypes() {
            return TypeCache
                .GetTypesDerivedFrom<IBlueprintNode>()
                .Where(t =>
                    (t.IsPublic || t.IsNestedPublic) && t.IsVisible &&
                    Attribute.IsDefined(t, typeof(BlueprintNodeAttribute)) &&
                    Attribute.IsDefined(t, typeof(SerializableAttribute))
                );
        }

        private static string GetNodeTypePath(Type nodeType) {
            var attr = GetBlueprintNodeAttribute(nodeType);
            string name = string.IsNullOrEmpty(attr.Name) ? nodeType.Name : attr.Name;
            string category = string.IsNullOrEmpty(attr.Category) ? "Other" : attr.Category;

            return $"{category}/{name}";
        }

        private static BlueprintNodeAttribute GetBlueprintNodeAttribute(Type type) {
            return type.GetCustomAttribute<BlueprintNodeAttribute>(false);
        }

        private static SearchTreeEntry ToSearchEntry<T>(TreeEntry<PathTree.Node<T>> treeEntry) {
            return treeEntry.children.Count == 0
                ? SearchTreeEntryUtils.Entry(treeEntry.data.name, treeEntry.data.data, treeEntry.level)
                : SearchTreeEntryUtils.Header(treeEntry.data.name, treeEntry.level);
        }
    }

}
