﻿using MisterGames.Common.Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(Map<,>), true)]
    public class MapPropertyDrawer : PropertyDrawer {
    
        private static GUIContent elementIndex;
        private ReorderableList _list;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (_list == null) {
                var listProp = property.FindPropertyRelative("_tuples");
                _list = new ReorderableList(
                    property.serializedObject, listProp, 
                    true, false, true, true
                ) {
                    drawElementCallback = DrawListItems
                };
            }
 
            var firstLine = position;
            firstLine.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(firstLine, property, label);
 
            if (property.isExpanded) {
                position.y += firstLine.height;
                if (elementIndex == null) elementIndex = new GUIContent();
                _list.DoList(position);
            }
        }
 
        private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused) {
            var element = _list.serializedProperty.GetArrayElementAtIndex(index);
            var keyProp   = element.FindPropertyRelative("Key");
            var valueProp = element.FindPropertyRelative("Value");
 
            elementIndex.text = $"Element {index}";
            EditorGUI.BeginProperty(rect, elementIndex, element);
            var prevLabelWidth = EditorGUIUtility.labelWidth;
 
            EditorGUIUtility.labelWidth = 75;
            var rect0 = rect; 
 
            var halfWidth = rect0.width / 2f;
            rect0.width = halfWidth;
            rect0.y += 1f;
            rect0.height -= 2f;
  
            EditorGUIUtility.labelWidth = 40;
 
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect0, keyProp);
 
            rect0.x += halfWidth + 4f;
 
            EditorGUI.PropertyField(rect0, valueProp);
 
            EditorGUIUtility.labelWidth = prevLabelWidth;
 
            EditorGUI.EndProperty();
        }
 
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (property.isExpanded) {
                var listProp = property.FindPropertyRelative("_tuples");
                return listProp.arraySize < 2
                    ? EditorGUIUtility.singleLineHeight + 52f
                    : EditorGUIUtility.singleLineHeight + 23f * listProp.arraySize + 29;
            }

            return EditorGUIUtility.singleLineHeight;
        }
        
    }
}