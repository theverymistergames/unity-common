using UnityEditor;

namespace MisterGames.Common.Editor.Drawers {

    public static class SerializedPropertyExtensions {

        public static void Draw(SerializedProperty property) {
            EditorGUILayout.PropertyField(property, true);
        }

        public static void DrawInline(SerializedProperty property) {
            while (property.NextVisible(true)) {
                EditorGUILayout.PropertyField(property, true);
            }
        }
    }

}
