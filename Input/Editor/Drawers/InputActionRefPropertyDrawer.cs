using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Views;
using MisterGames.Input.Actions;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(InputActionRef))]
    public sealed class InputActionRefPropertyDrawer : PropertyDrawer {
        
        private const string Separator = " : ";
        private const string Null = "<null>";
        private const string NotFound = "(not found)";

        private const string GuidPropertyPath = nameof(InputActionRef._guid);
        private const string GuidLowPropertyPath = "_guidLow";
        private const string GuidHighPropertyPath = "_guidHigh";

        private static readonly GUIContent NullLabel = new(Null);

        private readonly struct Entry {
            
            public readonly Guid guid;
            public readonly string path;
            
            public Entry(Guid guid, string path) {
                this.guid = guid;
                this.path = path;
            }
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            
            var rect = position;
            float indent = EditorGUI.indentLevel * 15;
            float offset = indent - 1f;
            rect.x += offset;
            rect.width -= offset;
            
            GUI.Label(rect, label);

            rect = position;
            offset = EditorGUIUtility.labelWidth + 2f;
            rect.x += offset;
            rect.width -= offset;
            
            var guidProperty = property.FindPropertyRelative(GuidPropertyPath);
            ulong guidLow = guidProperty.FindPropertyRelative(GuidLowPropertyPath).ulongValue;
            ulong guidHigh = guidProperty.FindPropertyRelative(GuidHighPropertyPath).ulongValue;

            var guid = HashHelpers.ComposeGuid(guidLow, guidHigh);
            
            if (EditorGUI.DropdownButton(rect, GetDropdownLabel(guid), FocusType.Keyboard)) {
                var dropdown = new AdvancedDropdown<Entry>(
                    "Select input action",
                    GetAllEntries(),
                    e => e.path ?? Null,
                    onItemSelected: (e, _) => {
                        var p = property.Copy();

                        var guidP = p.FindPropertyRelative(GuidPropertyPath);
                        (ulong low, ulong high) = HashHelpers.DecomposeGuid(e.guid);

                        guidP.FindPropertyRelative(GuidLowPropertyPath).ulongValue = low;
                        guidP.FindPropertyRelative(GuidHighPropertyPath).ulongValue = high;

                        p.serializedObject.ApplyModifiedProperties();
                        p.serializedObject.Update();
                    },
                    sort: nodes => nodes
                        .OrderBy(n => n.data.data.path),
                    pathToName: pathParts => string.Join(Separator, pathParts)
                );
                
                dropdown.Show(rect);
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        private static IEnumerable<Entry> GetAllEntries() {
            if (InputSystem.actions == null) {
                return new[] { default(Entry) };
            }
            
            return InputSystem.actions.actionMaps
                .SelectMany(m => m.actions)
                .Select(a => new Entry(a.id, GetInputActionPath(a)))
                .Prepend(default);
        }

        private static string GetInputActionPath(InputAction inputAction) {
            return inputAction.actionMap == null
                ? inputAction.name
                : $"{inputAction.actionMap.name}/{inputAction.name}";
        }

        private static GUIContent GetDropdownLabel(Guid guid) {
            if (InputSystem.actions == null || InputSystem.actions.FindAction(guid) is not { } action) {
                return guid == Guid.Empty 
                    ? NullLabel 
                    : new GUIContent($"{NotFound} guid {guid}");
            }

            return action.actionMap == null
                ? new GUIContent($"{action.name}")
                : new GUIContent($"{action.actionMap.name}{Separator}{action.name}");
        }
    }
    
}