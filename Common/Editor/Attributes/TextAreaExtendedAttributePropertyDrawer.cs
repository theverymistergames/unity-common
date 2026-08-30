using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Attributes {

    [CustomPropertyDrawer(typeof(TextAreaExtendedAttribute))]
    public class TextAreaExtendedAttributePropertyDrawer : PropertyDrawer {

        private const int MinLines = 3;
        private const int MaxLines = 20;

        private const float FallbackWidthOffset = 40f;
        private const float ScrollBarWidth = 16f;

        private readonly HashSet<int> _editedProperties = new();
        private readonly Dictionary<int, Vector2> _scrollPositions = new();

        // Width of the text area is known only while drawing, and the height is asked for before that:
        // the last drawn width is remembered here, so wrapped lines are measured by the width they are drawn at.
        private readonly Dictionary<int, float> _textAreaWidths = new();

        private static GUIStyle _textAreaStyle;

        private static GUIStyle TextAreaStyle => _textAreaStyle ??= new GUIStyle(EditorStyles.textArea) {
            wordWrap = true,
        };

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
                    DrawTextArea(GetTextAreaRect(position), property, hash, disabled: attr.showEditButtons && !isEdited);
                }
                else {
                    _editedProperties.Remove(hash);
                }
            }
            else {
                var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PrefixLabel(labelRect, label);

                DrawTextArea(GetTextAreaRect(position), property, hash, disabled: attr.showEditButtons && !isEdited);
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

        /// <summary>
        /// Text that does not fit into the area is scrolled instead of being cut off,
        /// so the area does not have to grow with the text.
        /// A disabled group of the ReadOnly attribute or of the Edit button would block the scrolling as well,
        /// so the area is drawn enabled, and the text that must not be changed is drawn as a selectable label:
        /// it can be scrolled and copied, but not edited.
        /// </summary>
        private void DrawTextArea(Rect rect, SerializedProperty property, int hash, bool disabled) {
            _textAreaWidths[hash] = rect.width;

            string text = property.stringValue ?? string.Empty;
            float contentHeight = GetContentHeight(text, rect.width - ScrollBarWidth);

            bool wasEnabled = GUI.enabled;
            bool canEdit = wasEnabled && !disabled;

            GUI.enabled = true;

            if (contentHeight <= rect.height) {
                DrawText(rect, property, canEdit);
            }
            else {
                var contentRect = new Rect(0f, 0f, rect.width - ScrollBarWidth, contentHeight);

                _scrollPositions.TryGetValue(hash, out var scrollPosition);

                scrollPosition = GUI.BeginScrollView(rect, scrollPosition, contentRect);
                DrawText(contentRect, property, canEdit);
                GUI.EndScrollView();

                _scrollPositions[hash] = scrollPosition;
            }

            GUI.enabled = wasEnabled;
        }

        private static void DrawText(Rect rect, SerializedProperty property, bool canEdit) {
            string text = property.stringValue ?? string.Empty;

            if (canEdit) {
                property.stringValue = EditorGUI.TextArea(rect, text, TextAreaStyle);
                return;
            }

            EditorGUI.SelectableLabel(rect, text, TextAreaStyle);
        }

        private static Rect GetTextAreaRect(Rect position) {
            float areaY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float areaHeight = position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;

            return new Rect(position.x, areaY, position.width, areaHeight);
        }

        /// <summary>
        /// Height of the text with the word wrap taken into account. CalcHeight of a style built by hand does not
        /// always wrap the lines and then returns the height of the raw lines only, so the height of a built in
        /// style that is known to wrap is used as the lower bound.
        /// </summary>
        private static float GetContentHeight(string text, float width) {
            var content = new GUIContent(text);
            float contentWidth = Mathf.Max(width, 1f);

            return Mathf.Max(
                TextAreaStyle.CalcHeight(content, contentWidth),
                EditorStyles.wordWrappedLabel.CalcHeight(content, contentWidth)
            );
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var attr = (TextAreaExtendedAttribute) attribute;

            if (attr.expandable && !property.isExpanded) return EditorGUIUtility.singleLineHeight;

            string text = property.stringValue ?? string.Empty;
            int hash = Animator.StringToHash(property.propertyPath);

            // Before the first draw the width is not known yet: the view width is close enough for one frame.
            float width = _textAreaWidths.TryGetValue(hash, out float drawnWidth)
                ? drawnWidth
                : EditorGUIUtility.currentViewWidth - FallbackWidthOffset;

            // Area never grows past the max amount of lines: a longer text is scrolled inside of it.
            float minHeight = EditorGUIUtility.singleLineHeight * MinLines;
            float maxHeight = EditorGUIUtility.singleLineHeight * MaxLines;
            float areaHeight = Mathf.Clamp(GetContentHeight(text, width - ScrollBarWidth), minHeight, maxHeight);

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + areaHeight;
        }
    }

}
