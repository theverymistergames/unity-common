using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Feedback.Editor {

    /// <summary>
    /// How much time players spent in the build: sessions and total playtime of the selected player,
    /// or a chart of the playtime of everyone when no player is selected.
    /// </summary>
    [Serializable]
    public sealed class FeedbackPlaytimeView : IFeedbackViewProvider {

        [Tooltip("Amount of players in the chart, the ones with the longest playtime are shown.")]
        [SerializeField] [Min(1)] private int _maxPlayersInChart = 20;
        [Tooltip("List the sessions of the selected player.")]
        [SerializeField] private bool _showSessions = true;
        [Tooltip("Amount of the last sessions in the list.")]
        [SerializeField] [Min(1)] private int _maxSessionsInList = 30;

        public string Title => "Playtime";

        public void OnGUI(in FeedbackViewContext context) {
            if (context.HasSelectedPlayer) DrawPlayer(context.selectedPlayer);
            else DrawAllPlayers(context.players);
        }

        private void DrawPlayer(FeedbackLogPlayer player) {
            var playtime = FeedbackViewGui.GetPlaytime(player);
            int sessionCount = player.sessions.Count;

            FeedbackViewGui.DrawStat("Player", $"{player.ShortId}{(string.IsNullOrEmpty(player.device) ? string.Empty : $"  ·  {player.device}")}");
            FeedbackViewGui.DrawStat("Sessions", sessionCount.ToString());
            FeedbackViewGui.DrawStat("Total playtime", FeedbackViewGui.FormatDuration(playtime));

            if (sessionCount > 0) {
                FeedbackViewGui.DrawStat("Average session",
                    FeedbackViewGui.FormatDuration(TimeSpan.FromSeconds(playtime.TotalSeconds / sessionCount)));

                FeedbackViewGui.DrawStat("Longest session",
                    FeedbackViewGui.FormatDuration(GetLongestSession(player)));

                FeedbackViewGui.DrawStat("First session",
                    $"{GetFirstTime(player).ToLocalTime():yyyy-MM-dd HH:mm}");

                FeedbackViewGui.DrawStat("Last session",
                    $"{player.lastTime.ToLocalTime():yyyy-MM-dd HH:mm}");
            }

            if (!_showSessions || sessionCount == 0) return;

            EditorGUILayout.Space(4f);
            FeedbackViewGui.DrawHeader("Sessions");

            var bars = new List<FeedbackViewGui.Bar>();

            // Sessions go from the oldest to the newest one, and the last ones are the interesting ones,
            // so a player with more sessions than the list holds is cut from the beginning.
            int start = Mathf.Max(sessionCount - _maxSessionsInList, 0);

            for (int i = start; i < sessionCount; i++) {
                var session = player.sessions[i];

                bars.Add(new FeedbackViewGui.Bar(
                    $"{session.startTime.ToLocalTime():yyyy-MM-dd HH:mm}",
                    (float) session.Duration.TotalSeconds,
                    $"{FeedbackViewGui.FormatDuration(session.Duration)}, {session.entries.Count} entries"));
            }

            FeedbackViewGui.DrawBars(bars);
        }

        private void DrawAllPlayers(IReadOnlyList<FeedbackLogPlayer> players) {
            int playerCount = players?.Count ?? 0;

            var total = TimeSpan.Zero;
            int sessionCount = 0;

            var sorted = new List<FeedbackLogPlayer>(playerCount);

            for (int i = 0; i < playerCount; i++) {
                var player = players![i];

                total += FeedbackViewGui.GetPlaytime(player);
                sessionCount += player.sessions.Count;

                sorted.Add(player);
            }

            FeedbackViewGui.DrawStat("Players", playerCount.ToString());
            FeedbackViewGui.DrawStat("Sessions", sessionCount.ToString());
            FeedbackViewGui.DrawStat("Total playtime", FeedbackViewGui.FormatDuration(total));

            if (playerCount > 0) {
                FeedbackViewGui.DrawStat("Average per player",
                    FeedbackViewGui.FormatDuration(TimeSpan.FromSeconds(total.TotalSeconds / playerCount)));
            }

            if (sessionCount > 0) {
                FeedbackViewGui.DrawStat("Average session",
                    FeedbackViewGui.FormatDuration(TimeSpan.FromSeconds(total.TotalSeconds / sessionCount)));
            }

            if (playerCount == 0) return;

            sorted.Sort((a, b) => FeedbackViewGui.GetPlaytime(b).CompareTo(FeedbackViewGui.GetPlaytime(a)));

            EditorGUILayout.Space(4f);
            FeedbackViewGui.DrawHeader("Playtime per player");

            var bars = new List<FeedbackViewGui.Bar>();
            int count = Mathf.Min(sorted.Count, _maxPlayersInChart);

            for (int i = 0; i < count; i++) {
                var player = sorted[i];
                var playtime = FeedbackViewGui.GetPlaytime(player);

                bars.Add(new FeedbackViewGui.Bar(
                    player.ShortId,
                    (float) playtime.TotalSeconds,
                    $"{FeedbackViewGui.FormatDuration(playtime)}, {player.sessions.Count} sessions"));
            }

            FeedbackViewGui.DrawBars(bars);

            if (sorted.Count > count) {
                EditorGUILayout.LabelField($"and {sorted.Count - count} players more", EditorStyles.miniLabel);
            }
        }

        private static TimeSpan GetLongestSession(FeedbackLogPlayer player) {
            var longest = TimeSpan.Zero;

            for (int i = 0; i < player.sessions.Count; i++) {
                var duration = player.sessions[i].Duration;
                if (duration > longest) longest = duration;
            }

            return longest;
        }

        private static DateTime GetFirstTime(FeedbackLogPlayer player) {
            return player.sessions.Count > 0 ? player.sessions[0].startTime : default;
        }
    }

}
