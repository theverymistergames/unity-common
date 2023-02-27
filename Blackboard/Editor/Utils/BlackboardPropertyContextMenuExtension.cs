using UnityEditor;
using UnityEngine;

namespace MisterGames.Blackboards.Editor {

    [InitializeOnLoad]
    internal static class BlackboardPropertyContextMenuExtension {

        static BlackboardPropertyContextMenuExtension() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            if (!BlackboardUtils.TryGetBlackboardProperty(property, out _, out var blackboard, out int hash)) return;

            menu.AddItem(new GUIContent("Reset"), false, () => {
                blackboard.TryResetPropertyValue(hash);

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            });
        }
    }

}
