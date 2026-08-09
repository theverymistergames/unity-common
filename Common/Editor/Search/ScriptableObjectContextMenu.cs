using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Search {

    [InitializeOnLoad]
    internal static class ScriptableObjectContextMenu {

        static ScriptableObjectContextMenu() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.ObjectReference ||
                property.objectReferenceValue is not ScriptableObject scriptableObject ||
                !AssetDatabase.Contains(scriptableObject)
               ) {
                return;
            }

            menu.AddItem(new GUIContent("Search usages..."), false, () => {
                ScriptableObjectSearchWindow.SearchScriptableObject(scriptableObject);
            });
        }
    }

}
