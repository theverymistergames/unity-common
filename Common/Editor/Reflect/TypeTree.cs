using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Lists;
using MisterGames.Common.Trees;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace MisterGames.Common.Editor.Reflect {

    public static class TypeTree {

        public static TreeEntry<Type> From(Type type) {
            var types = TypeCache.GetTypesDerivedFrom(type);
            return CreateEntry(type, types, 0);
        }
        
        public static TreeEntry<Type> From<T>() {
            return From(typeof(T));
        }

        public static IEnumerable<SearchTreeEntry> CreateSearchTree<T>(Func<Type, object> getData = null, int levelOffset = 0) {
            return CreateSearchTree(typeof(T), getData, levelOffset);
        }
        
        public static IEnumerable<SearchTreeEntry> CreateSearchTree(Type type, Func<Type, object> getData = null, int levelOffset = 0) {
            if (getData == null) getData = t => t;
            var getName = new Func<Type, string>(t => t.Name);
            return From(type)
                .RemoveNonVisibleLeafs()
                .RemoveAbstractBranches()
                .SelfChildNonAbstractBranches()
                .SortBranchesInChildrenFirst()
                .PreOrder()
                .RemoveRoot()
                .Select(e => e.ToSearchEntry(getName, getData, levelOffset));
        }
        
        public static TreeEntry<Type> SelfChildNonAbstractBranches(this TreeEntry<Type> entry) {
            if (entry.isLeaf) return entry;
            entry.children = entry.children.Select(SelfChildNonAbstractBranches).ToList();
            
            var selfChild = new TreeEntry<Type> {
                data = entry.data,
                level = entry.level + 1,
                isLeaf = true,
                children = new List<TreeEntry<Type>>()
            };

            if (!entry.IsAbstract() && !entry.children.Contains(selfChild)) {
                if (entry.children.IsEmpty()) entry.children.Add(selfChild);
                else entry.children.Insert(0, selfChild);
            }
            return entry;
        }
        
        public static TreeEntry<Type> RemoveNonVisibleLeafs(this TreeEntry<Type> entry) {
            return entry.RemoveLeafsIf(e => e.data.IsVisible);
        }

        public static TreeEntry<Type> RemoveAbstractLeafs(this TreeEntry<Type> entry) {
            return entry.RemoveLeafsIf(e => !e.IsAbstract() || !e.isLeaf);
        }

        public static TreeEntry<Type> RemoveAbstractBranches(this TreeEntry<Type> entry) {
            while (entry.AbstractLeafs().IsNotEmpty()) {
                entry = entry.RemoveAbstractLeafs();
            }
            return entry;
        }

        public static IEnumerable<TreeEntry<Type>> AbstractLeafs(this TreeEntry<Type> entry) {
            return entry.LevelOrder().Where(e => e.isLeaf && e.IsAbstract());
        }

        private static TreeEntry<Type> CreateEntry(Type type, TypeCache.TypeCollection types, int level) {
            var children = GetEntriesFor(type, types, level + 1);
            return new TreeEntry<Type> {
                data = type,
                children = children,
                isLeaf = children.IsEmpty(),
                level = level
            };
        }

        private static List<TreeEntry<Type>> GetEntriesFor(Type type, TypeCache.TypeCollection types, int level) {
            return GetDerivedTypes(type, types)
                    .Select(t => CreateEntry(t, types, level))
                    .ToList();
        }

        private static IEnumerable<Type> GetDerivedTypes(Type type, TypeCache.TypeCollection types) {
            return types.Where(t => {
                var tBase = t.BaseType;
                if (tBase == null) return type == null;
                
                var t1 = tBase.IsGenericType ? tBase.GetGenericTypeDefinition() : tBase;
                var t2 = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                
                return t1 == t2;
            });
        }

        private static bool IsAbstract(this TreeEntry<Type> entry) {
            return entry.data.IsAbstract || entry.data.IsInterface;
        }

    }

}