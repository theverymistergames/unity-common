using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Feedback.Editor {

    /// <summary>
    /// Errors, exceptions and failed assertions only, without the info entries.
    /// Same messages are folded into one row with the amount of times they happened
    /// and the amount of players who ran into them.
    /// </summary>
    [Serializable]
    public sealed class FeedbackErrorsView : IFeedbackViewProvider {

        [Tooltip("Fold the same messages into one row. Switch off to get a plain list in time order.")]
        [SerializeField] private bool _groupSameMessages = true;
        [Tooltip("Amount of rows shown.")]
        [SerializeField] [Min(1)] private int _maxRows = 50;
        [Tooltip("Amount of occurrences listed under a folded row.")]
        [SerializeField] [Min(1)] private int _maxOccurrences = 20;

        public string Title => "Errors";

        private sealed class ErrorGroup {
            public string message;
            public string type;
            public DateTime lastTime;
            public readonly List<FeedbackLogEntry> entries = new();
            public readonly HashSet<string> players = new();
        }

        [NonSerialized] private readonly HashSet<string> _expanded = new();

        public void OnGUI(in FeedbackViewContext context) {
            var errors = CollectErrors(context.players);

            if (errors.Count == 0) {
                EditorGUILayout.LabelField("no errors, exceptions or assertions", EditorStyles.miniLabel);
                return;
            }

            FeedbackViewGui.DrawStat("Errors", errors.Count.ToString());

            if (_groupSameMessages) DrawGroups(errors);
            else DrawList(errors);
        }

        private void DrawGroups(List<FeedbackLogEntry> errors) {
            var groups = GroupErrors(errors);

            FeedbackViewGui.DrawStat("Unique messages", groups.Count.ToString());
            EditorGUILayout.Space(4f);

            int shown = Mathf.Min(groups.Count, _maxRows);

            for (int i = 0; i < shown; i++) {
                var group = groups[i];
                string key = $"{group.type}/{group.message}";

                string title = $"{group.entries.Count} x  [{group.type}]  {GetFirstLine(group.message)}  ·  " +
                               $"{group.players.Count} {(group.players.Count == 1 ? "player" : "players")}  ·  " +
                               $"last {group.lastTime.ToLocalTime():yyyy-MM-dd HH:mm}";

                if (!DrawFoldout(key, title)) continue;

                EditorGUI.indentLevel++;

                int occurrences = Mathf.Min(group.entries.Count, _maxOccurrences);

                for (int j = 0; j < occurrences; j++) {
                    DrawEntry(group.entries[j]);
                }

                if (group.entries.Count > occurrences) {
                    EditorGUILayout.LabelField($"and {group.entries.Count - occurrences} more", EditorStyles.miniLabel);
                }

                EditorGUI.indentLevel--;
            }

            if (groups.Count > shown) {
                EditorGUILayout.LabelField($"and {groups.Count - shown} messages more", EditorStyles.miniLabel);
            }
        }

        private void DrawList(List<FeedbackLogEntry> errors) {
            EditorGUILayout.Space(4f);

            int shown = Mathf.Min(errors.Count, _maxRows);

            for (int i = 0; i < shown; i++) {
                DrawEntry(errors[i]);
            }

            if (errors.Count > shown) {
                EditorGUILayout.LabelField($"and {errors.Count - shown} errors more", EditorStyles.miniLabel);
            }
        }

        private void DrawEntry(FeedbackLogEntry entry) {
            string key = $"{entry.sessionId}/{entry.timeUtc}/{entry.message}";
            string player = string.IsNullOrEmpty(entry.playerId) ? "unknown" : entry.playerId;

            string title = $"{entry.time.ToLocalTime():yyyy-MM-dd HH:mm:ss}  ·  " +
                           $"{(player.Length <= 8 ? player : player[..8])}  ·  " +
                           $"[{entry.type}]  {GetFirstLine(entry.message)}";

            if (string.IsNullOrWhiteSpace(entry.stack)) {
                FeedbackViewGui.DrawRow(title, EditorStyles.miniLabel);
                return;
            }

            if (!DrawFoldout(key, title)) return;

            EditorGUI.indentLevel++;

            string text = $"{entry.message}\n{entry.stack}";
            FeedbackViewGui.DrawText(text);

            if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(50f))) {
                EditorGUIUtility.systemCopyBuffer = text;
            }

            EditorGUI.indentLevel--;
        }

        private bool DrawFoldout(string key, string title) {
            bool expanded = _expanded.Contains(key);
            bool result = EditorGUILayout.Foldout(expanded, title, toggleOnLabelClick: true, EditorStyles.foldout);

            if (result == expanded) return result;

            if (result) _expanded.Add(key);
            else _expanded.Remove(key);

            return result;
        }

        private static List<FeedbackLogEntry> CollectErrors(IReadOnlyList<FeedbackLogPlayer> players) {
            var errors = new List<FeedbackLogEntry>();

            for (int i = 0; i < players?.Count; i++) {
                var player = players[i];

                for (int j = 0; j < player.sessions.Count; j++) {
                    var session = player.sessions[j];

                    for (int k = 0; k < session.entries.Count; k++) {
                        var entry = session.entries[k];
                        if (entry.IsError) errors.Add(entry);
                    }
                }
            }

            errors.Sort((a, b) => b.time.CompareTo(a.time));

            return errors;
        }

        private static List<ErrorGroup> GroupErrors(List<FeedbackLogEntry> errors) {
            var groupMap = new Dictionary<string, ErrorGroup>();
            var groups = new List<ErrorGroup>();

            for (int i = 0; i < errors.Count; i++) {
                var entry = errors[i];
                string key = $"{entry.type}/{entry.message}";

                if (!groupMap.TryGetValue(key, out var group)) {
                    group = new ErrorGroup { message = entry.message, type = entry.type, lastTime = entry.time };

                    groupMap[key] = group;
                    groups.Add(group);
                }

                group.entries.Add(entry);
                group.players.Add(string.IsNullOrEmpty(entry.playerId) ? "unknown" : entry.playerId);

                if (entry.time > group.lastTime) group.lastTime = entry.time;
            }

            groups.Sort((a, b) => b.entries.Count.CompareTo(a.entries.Count));

            return groups;
        }

        private static string GetFirstLine(string message) {
            if (string.IsNullOrEmpty(message)) return string.Empty;

            int index = message.IndexOf('\n');
            return index < 0 ? message : message[..index];
        }
    }

}
