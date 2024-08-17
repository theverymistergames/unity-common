using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blackboards.Tables;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Common.Types;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Blackboards.Core.Blackboard;

namespace MisterGames.Blackboards.Editor {

    public static class BlackboardUtils {

        public static bool TryGetBlackboardPropertyData(SerializedProperty p, out BlackboardPropertyData data) {
            data = default;

            string path = p.propertyPath;
            Span<string> pathParts = path.Split('.');

            for (int i = pathParts.Length - 1; i > 0; i--) {
                string pathPart = pathParts[i];

                // Remove last path part: `some.property.path` => `some.property`
                int pathPartLength = pathPart.Length;
                path = path.Remove(path.Length - pathPartLength - 1, pathPartLength + 1);

                if (pathPart != "value") continue;

                // Search blackboard property hash near to value
                var hashProperty = p.serializedObject.FindProperty($"{path}.key");
                if (hashProperty is not { propertyType: SerializedPropertyType.Integer }) continue;

                if (i < 5) return false;

                // Reduce path to blackboard table
                int j = i - 1;
                for (; j > i - 5; j--) {
                    pathPart = pathParts[j];
                    pathPartLength = pathPart.Length;

                    path = path.Remove(path.Length - pathPartLength - 1, pathPartLength + 1);
                }

                if (p.serializedObject.FindProperty(path)?.GetValue() is not IBlackboardTable) return false;
                if (j < 5) return false;

                // Reduce path to blackboard
                int end = j - 5;
                for (; j > end; j--) {
                    pathPart = pathParts[j];
                    pathPartLength = pathPart.Length;

                    path = path.Remove(path.Length - pathPartLength - 1, pathPartLength + 1);
                }

                int hash = hashProperty.intValue;

                if (p.serializedObject.FindProperty(path)?.GetValue() is not Blackboard blackboard) return false;
                if (!blackboard.TryGetProperty(hash, out var blackboardProperty)) return false;

                data.hash = hash;
                data.blackboard = blackboard;
                data.property = blackboardProperty;

                return true;
            }

            return false;
        }

        public static BlackboardField CreateBlackboardPropertyView(BlackboardProperty property) {
            var type = property.type.ToType();
            string typeName = TypeNameFormatter.GetShortTypeName(type);
            string propertyName = property.name;

            return new BlackboardField { text = propertyName, typeText = typeName };
        }

        public static void NullPropertyField(Rect position, GUIContent label) {
            var labelRect = new Rect(
                position.x,
                position.y,
                EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.LabelField(labelRect, label);

            var valueRect = new Rect(
                position.x + EditorGUIUtility.labelWidth + 2f,
                position.y,
                position.width - EditorGUIUtility.labelWidth - 2f,
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.HelpBox(valueRect, $"Property type is null", MessageType.Warning);
        }

        public static float GetNullPropertyHeight() {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
