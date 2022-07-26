using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Lists;
using MisterGames.Common.Trees;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace MisterGames.Common.Editor.Reflect {

    public static class CategoryTree {

        public static TreeEntry<EntryData> From<T>(Func<Type, Meta> getMeta, Func<Type, bool> filter = null) {
            return From(typeof(T), getMeta, filter);
        }
        
        public static TreeEntry<EntryData> From(Type type, Func<Type, Meta> getMeta, Func<Type, bool> filter = null) {
            if (filter == null) filter = t => true;
            
            var metas = TypeCache
                .GetTypesDerivedFrom(type)
                .Where(t => t != type && !t.IsAbstract && !t.IsInterface && t.IsVisible && filter.Invoke(t))
                .Select(t => {
                    var meta = getMeta.Invoke(t);
                    var subs = meta.category
                        .Split('/')
                        .Select(s => s.Trim())
                        .Where(s => s.IsNotEmpty())
                        .ToArray();
                    
                    string category = subs.IsEmpty() ? "" : string.Join("/", subs);
                
                    return new MetaData {
                        name = meta.name, 
                        type = t,
                        category = category 
                    };
                })
                .ToList();
            
            return new TreeEntry<EntryData> {
                children = GetChildren("", metas, 0)
            };
        }

        public static IEnumerable<SearchTreeEntry> CreateSearchTree<T>(
            Func<Type, Meta> getMeta, 
            Func<Type, bool> filter = null,
            Func<Type, object> getData = null,
            int levelOffset = 0
        ) {
            return CreateSearchTree(typeof(T), getMeta, filter, getData, levelOffset);
        }
        
        public static IEnumerable<SearchTreeEntry> CreateSearchTree(
            Type type, 
            Func<Type, Meta> getMeta,
            Func<Type, bool> filter = null,
            Func<Type, object> getData = null, 
            int levelOffset = 0
        ) {
            var getEntryData = new Func<EntryData, object>(e => getData == null ? e.type : getData.Invoke(e.type));
            var getName = new Func<EntryData, string>(d => d.name);

            return From(type, getMeta, filter)
                .PreOrder()
                .RemoveRoot()
                .Select(e => e.ToSearchEntry(getName, getEntryData, levelOffset));
        }

        private static List<TreeEntry<EntryData>> GetChildren(string parent, IReadOnlyList<MetaData> metas, int level) {
            var entries = new List<TreeEntry<EntryData>>();
            var categories = new List<TreeEntry<EntryData>>();
            
            for (int i = 0; i < metas.Count; i++) {
                var meta = metas[i];

                if (!meta.category.IsSubCategoryOf(parent)) continue;

                if (parent == meta.category) {
                    entries.Add(new TreeEntry<EntryData> {
                        data = new EntryData { name = meta.name, type = meta.type },
                        level = level + 1,
                        isLeaf = true,
                        children = new List<TreeEntry<EntryData>>(),
                    });
                    continue;
                }
                
                var subs = meta.category.Split('/');
                string category = subs[level];

                if (categories.ContainsCategory(category)) continue;

                string fullCategory = string.Join("/", subs.Slice(0, level));
                
                categories.Add(new TreeEntry<EntryData> {
                    data = new EntryData { name = category },
                    level = level + 1,
                    isLeaf = false, 
                    children = GetChildren(fullCategory, metas, level + 1)
                });
            }
            
            return categories.OrderBy(e => e.data.name)
                .Plus(entries.OrderBy(e => e.data.name));
        }

        private static bool ContainsCategory(this IReadOnlyList<TreeEntry<EntryData>> categories, string category) {
            for (int j = 0; j < categories.Count; j++) {
                if (categories[j].data.name == category) return true;
            }
            return false;
        }
        
        private static bool IsSubCategoryOf(this string sub, string parent) {
            int parentLength = parent.Length;
            if (parentLength > sub.Length) return false;
            return parent == sub.Substring(0, parentLength);
        }
        
        public struct EntryData {
            public string name;
            public Type type;
        }

        public struct Meta {
            public string name;
            public string category;
        }

        private struct MetaData {
            public string name;
            public string category;
            public Type type;
        }
        
    }

}