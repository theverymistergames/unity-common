using System;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Blackboards {

    [InitializeOnLoad]
    internal static class BlackboardPropertyContextMenuExtension {

        static BlackboardPropertyContextMenuExtension() {
            EditorApplication.contextualPropertyMenu -= OnContextMenuOpening;
            EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
        }

        private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
            var serializedObject = property.serializedObject;

            string path = property.propertyPath;
            Span<string> pathParts = path.Split('.');

            Blackboard blackboard = null;
            int blackboardPropertyHash = 0;

            for (int i = pathParts.Length - 1; i > 0; i--) {
                string pathPart = pathParts[i];

                int pathPartLength = pathPart.Length;
                path = path.Remove(path.Length - pathPartLength - 1, pathPartLength + 1);

                if (pathPart != "value") continue;

                var hashProperty = serializedObject.FindProperty($"{path}.key");
                if (hashProperty is not { propertyType: SerializedPropertyType.Integer }) continue;

                if (i < 5) return;

                string pathCache = path;

                for (int j = i - 1; j > i - 5; j--) {
                    pathPart = pathParts[j];
                    pathPartLength = pathPart.Length;

                    path = path.Remove(path.Length - pathPartLength - 1, pathPartLength + 1);
                }

                var p = serializedObject.FindProperty(path);
                blackboard = p?.GetValue() as Blackboard;

                if (blackboard == null) {
                    path = pathCache;
                    continue;
                }

                blackboardPropertyHash = hashProperty.intValue;
                break;
            }

            if (blackboard == null) return;

            menu.AddItem(new GUIContent("Reset"), false, () => blackboard.TryResetPropertyValue(blackboardPropertyHash));
        }
    }

}
