using System;
using System.Collections.Generic;
using MisterGames.Common.Trees;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MisterGames.Common.Editor.Tree {

    public static class SearchTreeEntryUtils {

        public static SearchTreeEntry ToSearchEntry<T>(
            this TreeEntry<T> entry, 
            Func<T, string> getName,
            Func<T, object> getData,
            int levelOffset = 0
        ) {
            return entry.children.Count == 0
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
    }

}
