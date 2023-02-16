using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Lists;

namespace MisterGames.Common.Editor.Tree {

    public static class TypeTree {

        public static TreeEntry<Type> CreateEntry(Type type, Type[] types, int level) {
            return new TreeEntry<Type> {
                data = type,
                children = GetEntriesFor(type, types, level + 1),
                level = level
            };
        }
        
        public static TreeEntry<Type> SelfChildNonAbstractBranches(this TreeEntry<Type> entry) {
            if (entry.children.Count == 0) return entry;
            entry.children = entry.children.Select(SelfChildNonAbstractBranches).ToList();
            
            var selfChild = new TreeEntry<Type> {
                data = entry.data,
                level = entry.level + 1,
                children = new List<TreeEntry<Type>>()
            };

            if (!entry.IsAbstract() && !entry.children.Contains(selfChild)) {
                if (entry.children.Count == 0) entry.children.Add(selfChild);
                else entry.children.Insert(0, selfChild);
            }
            return entry;
        }

        public static TreeEntry<Type> RemoveAbstractLeafs(this TreeEntry<Type> entry) {
            return entry.RemoveLeafsIf(e => !e.IsAbstract() || e.children.Count > 0);
        }

        public static TreeEntry<Type> RemoveAbstractBranches(this TreeEntry<Type> entry) {
            while (!entry.AbstractLeafs().IsEmpty()) {
                entry = entry.RemoveAbstractLeafs();
            }
            return entry;
        }

        public static IEnumerable<TreeEntry<Type>> AbstractLeafs(this TreeEntry<Type> entry) {
            return entry.LevelOrder().Where(e => e.children.Count == 0 && e.IsAbstract());
        }

        private static List<TreeEntry<Type>> GetEntriesFor(Type type, Type[] types, int level) {
            return GetDerivedTypes(type, types)
                    .Select(t => CreateEntry(t, types, level))
                    .ToList();
        }

        private static IEnumerable<Type> GetDerivedTypes(Type type, IEnumerable<Type> types) {
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
