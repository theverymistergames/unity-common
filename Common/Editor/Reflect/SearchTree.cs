using System;
using System.Collections.Generic;
using MisterGames.Common.Trees;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MisterGames.Common.Editor.Reflect {

    public static class SearchTree {

        public static SearchTreeEntry ToSearchEntry<T>(
            this TreeEntry<T> entry, 
            Func<T, string> getName,
            Func<T, object> getData,
            int levelOffset = 0
        ) {
            return entry.isLeaf
                ? Entry(getName.Invoke(entry.data), getData.Invoke(entry.data), entry.level + levelOffset)
                : Header(getName.Invoke(entry.data), entry.level + levelOffset);
        }
        
        public static SearchTreeEntry Header(string title, int level = 0) {
            return new SearchTreeGroupEntry(new GUIContent(title), level);
        }

        public static SearchTreeEntry Entry(string title, object data, int level = 0) {
            // Search tree entry ident hack
            var ident = new Texture2D(1, 1);
            ident.SetPixel(0, 0, new UnityEngine.Color(0, 0, 0, 0));
            ident.Apply();
            
            return new SearchTreeEntry(new GUIContent(title, ident)) { userData = data, level = level };
        }

        public static Builder Create() {
            return new Builder();
        }
        
        public class Builder {

            private readonly List<SearchTreeEntry> _tree = new List<SearchTreeEntry>();

            public Builder Add(SearchTreeEntry entry) {
                _tree.Add(entry);
                return this;
            }
            
            public Builder Add(IEnumerable<SearchTreeEntry> entries) {
                _tree.AddRange(entries);
                return this;
            }

            public List<SearchTreeEntry> Build() {
                return _tree;
            }
            
        }

    }

}