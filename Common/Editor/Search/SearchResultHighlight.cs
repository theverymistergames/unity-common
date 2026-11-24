using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Search {

    public static class SearchResultHighlight {

        private static readonly Color ProSkinColor = new Color(0.24f, 0.49f, 0.91f, 0.35f);
        private static readonly Color LightSkinColor = new Color(0.24f, 0.49f, 0.91f, 0.3f);

        public static void Draw(Rect rect) {
            if (Event.current.type != EventType.Repaint) return;

            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? ProSkinColor : LightSkinColor);
        }
    }

}
