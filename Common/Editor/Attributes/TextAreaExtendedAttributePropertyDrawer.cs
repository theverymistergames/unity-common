using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes {

    [CustomPropertyDrawer(typeof(TextAreaExtendedAttribute))]
    public class TextAreaExtendedAttributePropertyDrawer : PropertyDrawer {

        private const int MinLines = 3;
        private readonly HashSet<int> _editedProperties = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var attr = (TextAreaExtendedAttribute) attribute;
            int hash = Animator.StringToHash(property.propertyPath);
            bool isEdited = _editedProperties.Contains(hash);
            
            float buttonWidth = 40f;
            float buttonHeight = EditorGUIUtility.singleLineHeight;
            var buttonRect = new Rect(position.xMax - buttonWidth, position.y, buttonWidth, buttonHeight);
            
            if (attr.expandable) {
                var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

                if (attr.showEditButtons) {
                    foldoutRect.width -= buttonWidth;
                }
                
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

                if (property.isExpanded) {
                    float areaY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    float areaHeight = position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
                    var areaRect = new Rect(position.x, areaY, position.width, areaHeight);

                    if (attr.showEditButtons) {
                        EditorGUI.BeginDisabledGroup(!isEdited);
                    }
                    
                    property.stringValue = EditorGUI.TextArea(areaRect, property.stringValue ?? string.Empty);
                    
                    if (attr.showEditButtons) {
                        EditorGUI.EndDisabledGroup();
                    }
                }
                else {
                    _editedProperties.Remove(hash);
                }
            }
            else {
                var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PrefixLabel(labelRect, label);
                
                float areaY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                float areaHeight = position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
                var areaRect = new Rect(position.x, areaY, position.width, areaHeight);
                
                if (attr.showEditButtons) {
                    EditorGUI.BeginDisabledGroup(!isEdited);
                }
                
                property.stringValue = EditorGUI.TextArea(areaRect, property.stringValue ?? string.Empty);
                
                if (attr.showEditButtons) {
                    EditorGUI.EndDisabledGroup();
                }
            }

            if (attr.showEditButtons) {
                if (!isEdited) {
                    if (GUI.Button(buttonRect, "Edit", EditorStyles.miniButton)) {
                        _editedProperties.Add(hash);
                        if (attr.expandable) property.isExpanded = true;
                    }   
                }
                else {
                    if (GUI.Button(buttonRect, "Done", EditorStyles.miniButton)) {
                        _editedProperties.Remove(hash);
                    }
                }   
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var attr = (TextAreaExtendedAttribute) attribute;

            if (attr.expandable && !property.isExpanded) return EditorGUIUtility.singleLineHeight;
            
            string text = property.stringValue ?? string.Empty;
            float minHeight = EditorGUIUtility.singleLineHeight * MinLines;
            float contentHeight = EditorStyles.textArea.CalcHeight(new GUIContent(text), EditorGUIUtility.currentViewWidth);
            float areaHeight = Mathf.Max(minHeight, contentHeight);

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + areaHeight;
        }
    }

}
