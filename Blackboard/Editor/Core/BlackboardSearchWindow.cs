using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Editor.Tree;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Blackboard = MisterGames.Blackboards.Core.Blackboard;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Editor {

    public class BlackboardSearchWindow : ScriptableObject, ISearchWindowProvider {

        public Action<SearchWindowContext> onSelectedArray = delegate {  };
        public Action<Type> onSelectType = delegate {  };

        private List<SearchTreeEntry> _typeTree;
        private bool _isPendingArrayType;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            _typeTree ??= CreateTypeTree();

            string title = _isPendingArrayType ? "Select element type" : "Select type";
            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header(title) };

            tree.AddRange(_typeTree);
            if (!_isPendingArrayType) tree.Add(SearchTreeEntryUtils.Entry("Array of ...", "array", 1));

            return tree;
        }

        private static List<SearchTreeEntry> CreateTypeTree() {
            var tree = new List<SearchTreeEntry>();

            var types = new[] {
                typeof(bool),

                typeof(float),
                typeof(double),

                typeof(int),
                typeof(long),

                typeof(string),

                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),

                typeof(Vector2Int),
                typeof(Vector3Int),

                typeof(Quaternion),

                typeof(Color),
                typeof(LayerMask),
            };
            tree.Add(SearchTreeEntryUtils.Header("Primitives", 1));
            tree.AddRange(types.Select(t => SearchTreeEntryUtils.Entry(TypeNameFormatter.GetTypeName(t), t, 2)));

            types = TypeCache
                .GetTypesDerivedFrom<ScriptableObject>()
                .Append(typeof(ScriptableObject))
                .Where(Blackboard.IsSupportedType)
                .ToArray();

            tree.AddRange(GetTypeTree("Scriptable Objects", types, 1));

            types = TypeCache
                .GetTypesDerivedFrom<Component>()
                .Append(typeof(Component))
                .Where(Blackboard.IsSupportedType)
                .ToArray();
            tree.AddRange(GetTypeTree("Components", types, 1));

            var type = typeof(GameObject);
            tree.Add(SearchTreeEntryUtils.Entry(TypeNameFormatter.GetTypeName(type), type, 1));

            types = TypeCache
                .GetTypesDerivedFrom<object>()
                .Where(t => !typeof(Object).IsAssignableFrom(t) && Blackboard.IsSupportedType(t))
                .ToArray();
            tree.AddRange(GetTypeTree("Objects", types, 1));

            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsInterface && Blackboard.IsSupportedType(t))
                .ToArray();
            tree.AddRange(GetTypeTree("Interfaces", types, 1));

            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsEnum && Blackboard.IsSupportedType(t))
                .ToArray();
            tree.AddRange(GetTypeTree("Enums", types, 1));

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is string s) {
                if (s == "array") {
                    _isPendingArrayType = true;
                    onSelectedArray.Invoke(context);
                    return true;
                }

                return false;
            }

            if (searchTreeEntry.userData is Type type) {
                if (_isPendingArrayType) {
                    _isPendingArrayType = false;
                    type = type.MakeArrayType();
                }

                onSelectType.Invoke(type);
                return true;
            }

            return false;
        }

        private static IEnumerable<SearchTreeEntry> GetTypeTree(string rootName, IReadOnlyList<Type> types, int level) {
            if (types.Count == 0) return new List<SearchTreeEntry>();

            if (types.Count == 1) {
                var type = types[0];
                return new List<SearchTreeEntry> { SearchTreeEntryUtils.Entry(type.Name, type, level) };
            }

            var pathTree = PathTree
                .CreateTree(types, t => t.FullName, '.')
                .PreOrder()
                .Where(e => e.level > 0)
                .Select(e => ToSearchEntry(e, level));

            var tree = new List<SearchTreeEntry> { SearchTreeEntryUtils.Header(rootName, level) };
            tree.AddRange(pathTree);
            return tree;
        }

        private static SearchTreeEntry ToSearchEntry<T>(TreeEntry<PathTree.Node<T>> treeEntry, int levelOffset) {
            return treeEntry.children.Count == 0
                ? SearchTreeEntryUtils.Entry(treeEntry.data.name, treeEntry.data.data, treeEntry.level + levelOffset)
                : SearchTreeEntryUtils.Header(treeEntry.data.name, treeEntry.level + levelOffset);
        }
    }

}
