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

        private const float BarRowHeight = 18f;
        private const float MinBarWidth = 2f;
        private const float ValueWidth = 110f;

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

                float barAreaWidth = Mathf.Max(rect.width - labelWidth - ValueWidth, 10f);

                var labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
                var backRect = new Rect(rect.x + labelWidth, rect.y + 3f, barAreaWidth, rect.height - 6f);
                var fillRect = new Rect(backRect.x, backRect.y,
                    Mathf.Max(barAreaWidth * (bar.value / max), MinBarWidth), backRect.height);
                var valueRect = new Rect(backRect.xMax + 6f, rect.y, ValueWidth - 6f, rect.height);

                GUI.Label(labelRect, bar.label, EditorStyles.miniLabel);
                EditorGUI.DrawRect(backRect, BarBackColor);
                EditorGUI.DrawRect(fillRect, BarColor);
                GUI.Label(valueRect, bar.valueText, EditorStyles.miniLabel);
            }
        }

        public static void DrawStat(string label, string value) {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, 16f));

            float labelWidth = Mathf.Min(rect.width * 0.5f, 220f);

            GUI.Label(new Rect(rect.x, rect.y, labelWidth, rect.height), label, EditorStyles.miniLabel);
            GUI.Label(new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height), value,
                EditorStyles.miniBoldLabel);
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
