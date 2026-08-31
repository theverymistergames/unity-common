using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Feedback.Editor {

    /// <summary>
    /// Performance of the build, built of the entries written by the performance log service:
    /// fps and memory over the session of the selected player with its hardware, or the same
    /// averaged per player when all players are shown.
    /// </summary>
    [Serializable]
    public sealed class FeedbackPerformanceView : IFeedbackViewProvider {

        [Tooltip("Amount of players in the charts, the slowest ones are shown first.")]
        [SerializeField] [Min(1)] private int _maxPlayersInChart = 20;
        [Tooltip("Amount of the last samples in the charts of the selected player.")]
        [SerializeField] [Min(1)] private int _maxSamplesInChart = 60;
        [Tooltip("Draw the memory charts.")]
        [SerializeField] private bool _showMemory = true;

        public string Title => "Performance";

        private const string LogHeader = "[PerformanceLog]";

        private static readonly Regex FpsRegex = new(
            @"FPS: (\d+) current, (\d+) average, (\d+) 1% low, (\d+) 0\.1% low", RegexOptions.IgnoreCase);

        private static readonly Regex AllocatedRegex = new(@"(\d+) MB allocated", RegexOptions.IgnoreCase);
        private static readonly Regex ManagedHeapRegex = new(@"(\d+) MB managed heap", RegexOptions.IgnoreCase);
        private static readonly Regex SystemMemoryRegex = new(@"(\d+) MB system", RegexOptions.IgnoreCase);
        private static readonly Regex SessionRegex = new(@"Session: (\d+) s", RegexOptions.IgnoreCase);
        private static readonly Regex SceneRegex = new(@"active scene \[([^\]]*)\]", RegexOptions.IgnoreCase);
        private static readonly Regex QualityRegex = new(@"Quality: ([^,]+)", RegexOptions.IgnoreCase);
        private static readonly Regex ScreenRegex = new(@"Screen: (\d+x\d+)", RegexOptions.IgnoreCase);

        private static readonly Regex CpuRegex = new(@"CPU: (.+)", RegexOptions.IgnoreCase);
        private static readonly Regex GpuRegex = new(@"GPU: (.+)", RegexOptions.IgnoreCase);
        private static readonly Regex RamRegex = new(@"RAM: (.+)", RegexOptions.IgnoreCase);
        private static readonly Regex VramRegex = new(@"VRAM: (.+)", RegexOptions.IgnoreCase);
        private static readonly Regex ApiRegex = new(@"Graphics API: (.+)", RegexOptions.IgnoreCase);
        private static readonly Regex OsRegex = new(@"OS: (.+)", RegexOptions.IgnoreCase);

        private struct Sample {
            public DateTime time;
            public int currentFps;
            public int averageFps;
            public int onePercentFps;
            public int zero1PercentFps;
            public int allocatedMb;
            public int managedHeapMb;
            public int systemMemoryMb;
            public string scene;
            public string quality;
            public string screen;
        }

        private sealed class Hardware {
            public string cpu;
            public string gpu;
            public string ram;
            public string vram;
            public string api;
            public string os;
        }

        public void OnGUI(in FeedbackViewContext context) {
            if (context.HasSelectedPlayer) DrawPlayer(context.selectedPlayer);
            else DrawAllPlayers(context.players);
        }

        private void DrawPlayer(FeedbackLogPlayer player) {
            var samples = new List<Sample>();
            var hardware = new Hardware();

            Collect(player, samples, hardware);

            if (samples.Count == 0) {
                DrawHardware(hardware);
                EditorGUILayout.LabelField("no performance entries for this player", EditorStyles.miniLabel);
                return;
            }

            DrawHardware(hardware);

            GetFpsStats(samples, out float averageFps, out int worstOnePercent, out int worstZero1Percent);
            GetMemoryStats(samples, out int averageAllocated, out int peakAllocated, out int peakManagedHeap);

            FeedbackViewGui.DrawStat("Samples", $"{samples.Count}, one per performance log");
            FeedbackViewGui.DrawStat("Average fps", $"{averageFps:0}");
            FeedbackViewGui.DrawStat("Worst 1% low", worstOnePercent.ToString());
            FeedbackViewGui.DrawStat("Worst 0.1% low", worstZero1Percent.ToString());

            if (peakAllocated > 0) {
                FeedbackViewGui.DrawStat("Allocated memory", $"{averageAllocated} MB average, {peakAllocated} MB peak");
            }

            if (peakManagedHeap > 0) FeedbackViewGui.DrawStat("Managed heap peak", $"{peakManagedHeap} MB");

            var last = samples[^1];

            if (!string.IsNullOrEmpty(last.quality)) FeedbackViewGui.DrawStat("Quality", last.quality);
            if (!string.IsNullOrEmpty(last.screen)) FeedbackViewGui.DrawStat("Screen", last.screen);

            int start = Mathf.Max(samples.Count - _maxSamplesInChart, 0);

            EditorGUILayout.Space(4f);
            FeedbackViewGui.DrawHeader("Fps over time");

            var bars = new List<FeedbackViewGui.Bar>(samples.Count - start);

            for (int i = start; i < samples.Count; i++) {
                var sample = samples[i];

                bars.Add(new FeedbackViewGui.Bar(
                    $"{sample.time.ToLocalTime():MM-dd HH:mm}",
                    sample.averageFps,
                    $"{sample.averageFps} avg, {sample.onePercentFps} 1%, {sample.zero1PercentFps} 0.1%" +
                    (string.IsNullOrEmpty(sample.scene) ? string.Empty : $", {sample.scene}")));
            }

            FeedbackViewGui.DrawBars(bars, labelWidth: 110f);

            if (!_showMemory || peakAllocated <= 0) return;

            EditorGUILayout.Space(4f);
            FeedbackViewGui.DrawHeader("Allocated memory over time");

            bars.Clear();

            for (int i = start; i < samples.Count; i++) {
                var sample = samples[i];

                bars.Add(new FeedbackViewGui.Bar(
                    $"{sample.time.ToLocalTime():MM-dd HH:mm}",
                    sample.allocatedMb,
                    $"{sample.allocatedMb} MB, heap {sample.managedHeapMb} MB"));
            }

            FeedbackViewGui.DrawBars(bars, labelWidth: 110f);
        }

        private void DrawAllPlayers(IReadOnlyList<FeedbackLogPlayer> players) {
            var stats = new List<(FeedbackLogPlayer player, float averageFps, int worstOnePercent, int peakAllocated, Hardware hardware, int samples)>();

            var samples = new List<Sample>();
            int totalSamples = 0;

            for (int i = 0; i < players?.Count; i++) {
                var player = players[i];
                var hardware = new Hardware();

                samples.Clear();
                Collect(player, samples, hardware);

                if (samples.Count == 0) continue;

                GetFpsStats(samples, out float averageFps, out int worstOnePercent, out _);
                GetMemoryStats(samples, out _, out int peakAllocated, out _);

                totalSamples += samples.Count;
                stats.Add((player, averageFps, worstOnePercent, peakAllocated, hardware, samples.Count));
            }

            FeedbackViewGui.DrawStat("Players with performance logs", $"{stats.Count} of {players?.Count ?? 0}");

            if (stats.Count == 0) {
                EditorGUILayout.LabelField("no performance entries", EditorStyles.miniLabel);
                return;
            }

            FeedbackViewGui.DrawStat("Samples", totalSamples.ToString());

            float sum = 0f;
            for (int i = 0; i < stats.Count; i++) {
                sum += stats[i].averageFps;
            }

            FeedbackViewGui.DrawStat("Average fps over players", $"{sum / stats.Count:0}");

            // The slowest players are the ones worth looking at, so they go first.
            stats.Sort((a, b) => a.averageFps.CompareTo(b.averageFps));

            EditorGUILayout.Space(4f);
            FeedbackViewGui.DrawHeader("Average fps per player");

            var bars = new List<FeedbackViewGui.Bar>();
            int shown = Mathf.Min(stats.Count, _maxPlayersInChart);

            for (int i = 0; i < shown; i++) {
                var stat = stats[i];

                bars.Add(new FeedbackViewGui.Bar(
                    stat.player.ShortId,
                    stat.averageFps,
                    $"{stat.averageFps:0} avg, {stat.worstOnePercent} worst 1%" +
                    (string.IsNullOrEmpty(stat.hardware.gpu) ? string.Empty : $", {stat.hardware.gpu}")));
            }

            FeedbackViewGui.DrawBars(bars);

            if (stats.Count > shown) {
                EditorGUILayout.LabelField($"and {stats.Count - shown} players more", EditorStyles.miniLabel);
            }

            if (!_showMemory) return;

            EditorGUILayout.Space(4f);
            FeedbackViewGui.DrawHeader("Peak allocated memory per player");

            stats.Sort((a, b) => b.peakAllocated.CompareTo(a.peakAllocated));

            bars.Clear();
            shown = Mathf.Min(stats.Count, _maxPlayersInChart);

            for (int i = 0; i < shown; i++) {
                var stat = stats[i];

                bars.Add(new FeedbackViewGui.Bar(
                    stat.player.ShortId,
                    stat.peakAllocated,
                    $"{stat.peakAllocated} MB" +
                    (string.IsNullOrEmpty(stat.hardware.ram) ? string.Empty : $" of {stat.hardware.ram}")));
            }

            FeedbackViewGui.DrawBars(bars);
        }

        private static void DrawHardware(Hardware hardware) {
            if (!string.IsNullOrEmpty(hardware.cpu)) FeedbackViewGui.DrawStat("CPU", hardware.cpu);
            if (!string.IsNullOrEmpty(hardware.gpu)) FeedbackViewGui.DrawStat("GPU", hardware.gpu);
            if (!string.IsNullOrEmpty(hardware.ram)) FeedbackViewGui.DrawStat("RAM", hardware.ram);
            if (!string.IsNullOrEmpty(hardware.vram)) FeedbackViewGui.DrawStat("VRAM", hardware.vram);
            if (!string.IsNullOrEmpty(hardware.api)) FeedbackViewGui.DrawStat("Graphics API", hardware.api);
            if (!string.IsNullOrEmpty(hardware.os)) FeedbackViewGui.DrawStat("OS", hardware.os);
        }

        /// <summary>
        /// Reads the entries of the performance log service back: the periodic block becomes a sample,
        /// the block written on start fills the hardware of the player.
        /// </summary>
        private static void Collect(FeedbackLogPlayer player, List<Sample> samples, Hardware hardware) {
            // Sessions go from the oldest to the newest one, and the hardware of the newest one
            // is the one that describes the player now.
            for (int i = 0; i < player.sessions.Count; i++) {
                var session = player.sessions[i];

                for (int j = 0; j < session.entries.Count; j++) {
                    var entry = session.entries[j];

                    if (entry.message == null || !entry.message.StartsWith(LogHeader, StringComparison.Ordinal)) continue;

                    ReadHardware(entry.message, hardware);

                    var fps = FpsRegex.Match(entry.message);
                    if (!fps.Success) continue;

                    samples.Add(new Sample {
                        time = entry.time,
                        currentFps = ParseInt(fps.Groups[1].Value),
                        averageFps = ParseInt(fps.Groups[2].Value),
                        onePercentFps = ParseInt(fps.Groups[3].Value),
                        zero1PercentFps = ParseInt(fps.Groups[4].Value),
                        allocatedMb = ReadInt(AllocatedRegex, entry.message),
                        managedHeapMb = ReadInt(ManagedHeapRegex, entry.message),
                        systemMemoryMb = ReadInt(SystemMemoryRegex, entry.message),
                        scene = ReadString(SceneRegex, entry.message),
                        quality = ReadString(QualityRegex, entry.message),
                        screen = ReadString(ScreenRegex, entry.message),
                    });
                }
            }

            samples.Sort((a, b) => a.time.CompareTo(b.time));
        }

        private static void ReadHardware(string message, Hardware hardware) {
            hardware.cpu = ReadString(CpuRegex, message) ?? hardware.cpu;
            hardware.gpu = ReadString(GpuRegex, message) ?? hardware.gpu;
            hardware.ram = ReadString(RamRegex, message) ?? hardware.ram;
            hardware.vram = ReadString(VramRegex, message) ?? hardware.vram;
            hardware.api = ReadString(ApiRegex, message) ?? hardware.api;
            hardware.os = ReadString(OsRegex, message) ?? hardware.os;
        }

        private static void GetFpsStats(
            List<Sample> samples,
            out float averageFps,
            out int worstOnePercent,
            out int worstZero1Percent)
        {
            float sum = 0f;

            worstOnePercent = int.MaxValue;
            worstZero1Percent = int.MaxValue;

            for (int i = 0; i < samples.Count; i++) {
                var sample = samples[i];

                sum += sample.averageFps;

                if (sample.onePercentFps < worstOnePercent) worstOnePercent = sample.onePercentFps;
                if (sample.zero1PercentFps < worstZero1Percent) worstZero1Percent = sample.zero1PercentFps;
            }

            averageFps = samples.Count > 0 ? sum / samples.Count : 0f;

            if (worstOnePercent == int.MaxValue) worstOnePercent = 0;
            if (worstZero1Percent == int.MaxValue) worstZero1Percent = 0;
        }

        private static void GetMemoryStats(
            List<Sample> samples,
            out int averageAllocated,
            out int peakAllocated,
            out int peakManagedHeap)
        {
            long sum = 0;

            peakAllocated = 0;
            peakManagedHeap = 0;

            for (int i = 0; i < samples.Count; i++) {
                var sample = samples[i];

                sum += sample.allocatedMb;

                if (sample.allocatedMb > peakAllocated) peakAllocated = sample.allocatedMb;
                if (sample.managedHeapMb > peakManagedHeap) peakManagedHeap = sample.managedHeapMb;
            }

            averageAllocated = samples.Count > 0 ? (int) (sum / samples.Count) : 0;
        }

        private static string ReadString(Regex regex, string message) {
            var match = regex.Match(message);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private static int ReadInt(Regex regex, string message) {
            var match = regex.Match(message);
            return match.Success ? ParseInt(match.Groups[1].Value) : 0;
        }

        private static int ParseInt(string value) {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : 0;
        }
    }

}
