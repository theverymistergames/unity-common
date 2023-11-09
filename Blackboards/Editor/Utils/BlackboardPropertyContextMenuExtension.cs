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
            if (!BlackboardUtils.TryGetBlackboardPropertyData(property, out var data)) return;

            menu.AddItem(new GUIContent("Reset"), false, () => {
                data.blackboard.TryResetPropertyValue(data.hash);

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            });
        }
    }

}
