using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Feedback.Editor {

    /// <summary>
    /// Drawing shared by the feedback views: bars, stat lines and time formatting.
    /// </summary>
    public static class FeedbackViewGui {

        public readonly struct Bar {

            public readonly string label;
            public readonly float value;
            public readonly string valueText;

            public Bar(string label, float value, string valueText) {
                this.label = label;
                this.value = value;
                this.valueText = valueText;
            }
        }

        /// <summary>
        /// Distance the text of a row has to be moved by to start where the text of a foldout starts:
        /// a foldout keeps the room of its arrow on the left, a plain row does not.
        /// Both paddings are read from the styles, so a skin with another arrow keeps the rows lined up.
        /// </summary>
        public static float GetFoldoutTextOffset(GUIStyle rowStyle = null) {
            rowStyle ??= EditorStyles.label;
            return Mathf.Max(EditorStyles.foldout.padding.left - rowStyle.padding.left, 0f);
        }

        /// <summary>
        /// Foldout of the same look as the plain one, but with a bold text.
        /// </summary>
        public static GUIStyle BoldFoldout =>
            _boldFoldout ??= new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };

        private static GUIStyle _boldFoldout;

        private const float BarRowHeight = 18f;
        private const float MinBarWidth = 2f;
        private const float MinRowWidth = 10f;
        private const float MinBarAreaWidth = 40f;
        private const float MinValueWidth = 110f;

        /// <summary>
        /// Parts of the row the value text and the label are allowed to take when the window is narrow:
        /// a value of a bar is the interesting half of it, and a hardware name never fits anyway.
        /// </summary>
        private const float ValueWidthPart = 0.4f;
        private const float LabelWidthPart = 0.3f;
        private const float StatLabelPart = 0.35f;
        private const float MaxStatLabelWidth = 190f;

        private static readonly Color BarColor = new(0.35f, 0.6f, 0.9f, 0.85f);
        private static readonly Color BarBackColor = new(1f, 1f, 1f, 0.05f);

        public static void DrawBars(IReadOnlyList<Bar> bars, float labelWidth = 150f) {
            int count = bars?.Count ?? 0;

            if (count == 0) {
                EditorGUILayout.LabelField("no data", EditorStyles.miniLabel);
                return;
            }

            float max = 0f;

            for (int i = 0; i < count; i++) {
                max = Mathf.Max(max, bars![i].value);
            }

            if (max <= 0f) max = 1f;

            for (int i = 0; i < count; i++) {
                var bar = bars![i];
                var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, BarRowHeight));

                // The columns follow the width of the window: a fixed value column cuts the text
                // of a bar as soon as the window is not wide enough for it.
                float labels = Mathf.Min(labelWidth, rect.width * LabelWidthPart);
                float rest = Mathf.Max(rect.width - labels, MinRowWidth);
                float valueWidth = Mathf.Min(Mathf.Max(MinValueWidth, rest * ValueWidthPart),
                    Mathf.Max(rest - MinBarAreaWidth, 0f));
                float barAreaWidth = Mathf.Max(rest - valueWidth, MinBarAreaWidth);

                var labelRect = new Rect(rect.x, rect.y, labels, rect.height);
                var backRect = new Rect(rect.x + labels, rect.y + 3f, barAreaWidth, rect.height - 6f);
                var fillRect = new Rect(backRect.x, backRect.y,
                    Mathf.Max(barAreaWidth * (bar.value / max), MinBarWidth), backRect.height);
                var valueRect = new Rect(backRect.xMax + 6f, rect.y, Mathf.Max(valueWidth - 6f, MinRowWidth), rect.height);

                // The full text is kept in the tooltip: a gpu name fits into no column of a docked window.
                GUI.Label(labelRect, new GUIContent(bar.label, bar.label), EditorStyles.miniLabel);
                EditorGUI.DrawRect(backRect, BarBackColor);
                EditorGUI.DrawRect(fillRect, BarColor);
                GUI.Label(valueRect, new GUIContent(bar.valueText, bar.valueText), EditorStyles.miniLabel);
            }
        }

        /// <summary>
        /// A row of text that takes the whole width it is given.
        /// EditorGUILayout.LabelField keeps the width of a prefix label for itself and cuts the text
        /// at it, which is what makes a long message end in the middle of a word.
        /// </summary>
        public static void DrawRow(string text, GUIStyle style = null, bool alignWithFoldout = true) {
            style ??= EditorStyles.label;

            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight));

            if (alignWithFoldout) {
                float offset = GetFoldoutTextOffset(style);

                rect.x += offset;
                rect.width = Mathf.Max(rect.width - offset, MinRowWidth);
            }

            GUI.Label(rect, text, style);
        }

        /// <summary>
        /// Text of several lines, wrapped and selectable. The height is asked from the layout,
        /// so the text is never cut on the right and never leaves an empty gap under itself.
        /// </summary>
        public static void DrawText(string text, GUIStyle style = null) {
            style ??= WrappedLabel;

            var content = new GUIContent(text);
            var rect = GUILayoutUtility.GetRect(content, style, GUILayout.ExpandWidth(true));

            EditorGUI.SelectableLabel(EditorGUI.IndentedRect(rect), text, style);
        }

        public static GUIStyle WrappedLabel =>
            _wrappedLabel ??= new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };

        private static GUIStyle _wrappedLabel;

        public static void DrawStat(string label, string value) {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, 16f));

            float labelWidth = Mathf.Min(rect.width * StatLabelPart, MaxStatLabelWidth);
            float valueWidth = Mathf.Max(rect.width - labelWidth, MinRowWidth);

            GUI.Label(new Rect(rect.x, rect.y, labelWidth, rect.height), label, EditorStyles.miniLabel);
            GUI.Label(new Rect(rect.x + labelWidth, rect.y, valueWidth, rect.height),
                new GUIContent(value, value), EditorStyles.miniBoldLabel);
        }

        public static void DrawHeader(string title) {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        public static string FormatDuration(TimeSpan duration) {
            if (duration.TotalHours >= 1d) return $"{(int) duration.TotalHours} h {duration.Minutes} min";
            if (duration.TotalMinutes >= 1d) return $"{(int) duration.TotalMinutes} min {duration.Seconds} s";

            return $"{(int) duration.TotalSeconds} s";
        }

        /// <summary>
        /// Time of the session: from its first entry to its last one. A session that was killed
        /// without a quit entry is measured by the last entry that reached the table.
        /// </summary>
        public static TimeSpan GetPlaytime(FeedbackLogPlayer player) {
            var playtime = TimeSpan.Zero;

            for (int i = 0; i < player.sessions.Count; i++) {
                playtime += player.sessions[i].Duration;
            }

            return playtime;
        }
    }

}
