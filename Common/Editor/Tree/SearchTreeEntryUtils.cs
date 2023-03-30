using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MisterGames.Common.Editor.Tree {

    public static class SearchTreeEntryUtils {

        public static SearchTreeEntry Header(string title, int level = 0) {
            return new SearchTreeGroupEntry(new GUIContent(title), level);
        }

        public static SearchTreeEntry Entry(string title, object data, int level = 0) {
            // Search tree entry ident hack
            var indent = new Texture2D(1, 1);
            indent.SetPixel(0, 0, new UnityEngine.Color(0, 0, 0, 0));
            indent.Apply();

            return new SearchTreeEntry(new GUIContent(title, indent)) { userData = data, level = level };
        }
    }

}
