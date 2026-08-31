using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Feedback.Editor {

    /// <summary>
    /// Reads the feedback table written by the Apps Script web app and shows it grouped by players
    /// and their sessions. Where to read it from and what to read it with is set in the analyzer block
    /// of <see cref="FeedbackServiceConfig"/>. The table is downloaded with the service account
    /// of the project, so the account needs read access to the feedback spreadsheet.
    /// </summary>
    public sealed class FeedbackLogAnalyzerWindow : EditorWindow {

        private const string WindowTitle = "Feedback Logs";

        private const string ConfigKey = "MisterGames.Feedback.Analyzer.Config";
        private const string DisabledPlatformsKey = "MisterGames.Feedback.Analyzer.DisabledPlatforms";
        private const string UnknownPlatform = "unknown";

        private const float PlayerListWidth = 260f;

        private FeedbackServiceConfig _config;

        [Tooltip("Custom views of the logs, drawn above the log tree in the order they are set.")]
        [SerializeReference] [SubclassSelector(includeEditor: true)] private IFeedbackViewProvider[] _views;

        private SerializedObject _serializedObject;
        private SerializedProperty _viewsProperty;

        private FeedbackLogEntry[] _entries = Array.Empty<FeedbackLogEntry>();
        private List<FeedbackLogPlayer> _players = new();

        private readonly HashSet<string> _expanded = new();
        private readonly List<FeedbackLogPlayer> _selectedPlayers = new(1);
        private readonly List<string> _platforms = new();
        private readonly Dictionary<string, int> _platformCounts = new();
        private readonly HashSet<string> _disabledPlatforms = new();

        private int _visibleEntryCount;
        private CancellationTokenSource _cts;

        private string _selectedPlayerId;
        private string _search = string.Empty;
        private bool _errorsOnly;
        private bool _showSettings;
        private bool _isDownloading;
        private string _status;

        private Vector2 _playersScroll;
        private Vector2 _logsScroll;

        [MenuItem("MisterGames/Feedback Log Analyzer")]
        private static void OpenWindow() {
            GetWindow<FeedbackLogAnalyzerWindow>(WindowTitle, focus: true);
        }

        private void OnEnable() {
            _serializedObject = new SerializedObject(this);
            _viewsProperty = _serializedObject.FindProperty(nameof(_views));

            ReadPrefs();

            SetEntries(FeedbackLogLoader.ReadCache(out string downloadedAt));

            _status = _entries.Length > 0
                ? $"{_entries.Length} entries from the cache, downloaded {downloadedAt}"
                : "no logs downloaded yet";

            _showSettings = _entries.Length == 0;
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _cts);
            WritePrefs();
        }

        private void OnGUI() {
            DrawToolbar();
            if (_showSettings) DrawSettings();

            EditorGUILayout.BeginHorizontal();
            {
                DrawPlayers();
                DrawLogs();
            }
            EditorGUILayout.EndHorizontal();

            DrawStatus();
        }

        private void DrawToolbar() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                using (new EditorGUI.DisabledScope(_isDownloading)) {
                    if (GUILayout.Button("Download", EditorStyles.toolbarButton, GUILayout.Width(80f))) {
                        Download().Forget();
                    }
                }

                if (_isDownloading && GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.Width(60f))) {
                    AsyncExt.DisposeCts(ref _cts);
                    _isDownloading = false;
                    _status = "download is cancelled";
                }

                _showSettings = GUILayout.Toggle(_showSettings, "Settings", EditorStyles.toolbarButton, GUILayout.Width(70f));

                GUILayout.Space(10f);

                GUILayout.Label("Search", GUILayout.Width(46f));
                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(120f));

                _errorsOnly = GUILayout.Toggle(_errorsOnly, "Errors only", EditorStyles.toolbarButton, GUILayout.Width(80f));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Collapse all", EditorStyles.toolbarButton, GUILayout.Width(90f))) {
                    _expanded.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSettings() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                _config = EditorGUILayout.ObjectField(
                    new GUIContent("Config", "Feedback service config with the analyzer block filled in."),
                    _config, typeof(FeedbackServiceConfig), allowSceneObjects: false) as FeedbackServiceConfig;

                if (_config == null) {
                    EditorGUILayout.HelpBox($"Create a {nameof(FeedbackServiceConfig)} asset and set it here.",
                        MessageType.Warning);
                }
                else {
                    using (new EditorGUI.DisabledScope(true)) {
                        EditorGUILayout.ObjectField("Credentials", _config.AnalyzerCredentials,
                            typeof(TextAsset), allowSceneObjects: false);

                        EditorGUILayout.TextField("Spreadsheet id", _config.AnalyzerSpreadsheetId);
                        EditorGUILayout.TextField("Sheet name", _config.AnalyzerSheetName);
                    }

                    if (GetSettingsError() is { } error) EditorGUILayout.HelpBox(error, MessageType.Warning);

                    if (GUILayout.Button("Edit config", GUILayout.Width(100f))) {
                        Selection.activeObject = _config;
                        EditorGUIUtility.PingObject(_config);
                    }
                }

                EditorGUILayout.Space(4f);

                _serializedObject.Update();
                EditorGUILayout.PropertyField(_viewsProperty, new GUIContent("Views"), includeChildren: true);
                _serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();
        }

        private string GetSettingsError() {
            if (_config == null) return $"{nameof(FeedbackServiceConfig)} is not set.";
            if (_config.AnalyzerCredentials == null) return "Analyzer credentials are not set in the config.";
            if (string.IsNullOrWhiteSpace(_config.AnalyzerSpreadsheetId)) return "Analyzer spreadsheet id is not set in the config.";
            if (string.IsNullOrWhiteSpace(_config.AnalyzerSheetName)) return "Analyzer sheet name is not set in the config.";

            return null;
        }

        private void DrawPlayers() {
            EditorGUILayout.BeginVertical(GUILayout.Width(PlayerListWidth));
            {
                _playersScroll = EditorGUILayout.BeginScrollView(_playersScroll, EditorStyles.helpBox);
                {
                    DrawPlayerRow(null, $"All players ({_players.Count})", _visibleEntryCount, CountErrors());

                    for (int i = 0; i < _players.Count; i++) {
                        var player = _players[i];

                        DrawPlayerRow(player.playerId,
                            $"{player.ShortId}  ·  {player.sessions.Count} sessions",
                            player.entryCount, player.errorCount);
                    }
                }
                EditorGUILayout.EndScrollView();

                DrawPlatforms();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPlatforms() {
            if (_platforms.Count == 0) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Platforms", EditorStyles.boldLabel);

                for (int i = 0; i < _platforms.Count; i++) {
                    string platform = _platforms[i];
                    bool isEnabled = !_disabledPlatforms.Contains(platform);

                    bool result = EditorGUILayout.ToggleLeft(
                        $"{platform}  ({_platformCounts.GetValueOrDefault(platform)})", isEnabled);

                    if (result == isEnabled) continue;

                    if (result) _disabledPlatforms.Remove(platform);
                    else _disabledPlatforms.Add(platform);

                    RebuildPlayers();
                }

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("All", EditorStyles.miniButtonLeft)) {
                        _disabledPlatforms.Clear();
                        RebuildPlayers();
                    }

                    if (GUILayout.Button("None", EditorStyles.miniButtonRight)) {
                        _disabledPlatforms.Clear();

                        for (int i = 0; i < _platforms.Count; i++) {
                            _disabledPlatforms.Add(_platforms[i]);
                        }

                        RebuildPlayers();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawPlayerRow(string playerId, string title, int entryCount, int errorCount) {
            bool isSelected = _selectedPlayerId == playerId;

            var style = isSelected ? EditorStyles.boldLabel : EditorStyles.label;
            var rect = EditorGUILayout.GetControlRect(false, 32f);

            if (isSelected) EditorGUI.DrawRect(rect, new Color(0.3f, 0.5f, 0.8f, 0.2f));

            var titleRect = new Rect(rect.x + 4f, rect.y, rect.width - 8f, rect.height * 0.5f);
            var infoRect = new Rect(rect.x + 4f, rect.y + rect.height * 0.5f, rect.width - 8f, rect.height * 0.5f);

            GUI.Label(titleRect, title, style);
            GUI.Label(infoRect, $"{entryCount} entries, {errorCount} errors", EditorStyles.miniLabel);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
                _selectedPlayerId = playerId;

                Event.current.Use();
                Repaint();
            }
        }

        private void DrawLogs() {
            EditorGUILayout.BeginVertical();
            {
                _logsScroll = EditorGUILayout.BeginScrollView(_logsScroll);
                {
                    DrawViews();

                    for (int i = 0; i < _players.Count; i++) {
                        var player = _players[i];

                        if (_selectedPlayerId != null && player.playerId != _selectedPlayerId) continue;

                        DrawPlayer(player);
                    }

                    if (_players.Count == 0) {
                        EditorGUILayout.HelpBox(
                            $"No logs. Fill the analyzer block of the {nameof(FeedbackServiceConfig)} asset " +
                            "and press Download.",
                            MessageType.Info);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawViews() {
            int count = _views?.Length ?? 0;
            if (count == 0) return;

            var context = new FeedbackViewContext(GetSelectedPlayers(), GetSelectedPlayer(), _search, _errorsOnly);

            for (int i = 0; i < count; i++) {
                var view = _views![i];
                if (view == null) continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    FeedbackViewGui.DrawHeader(view.Title);

                    // A broken view must not take the whole window down with it.
                    try {
                        view.OnGUI(context);
                    }
                    catch (ExitGUIException) {
                        throw;
                    }
                    catch (Exception e) {
                        EditorGUILayout.HelpBox($"{view.GetType().Name} failed: {e.Message}", MessageType.Error);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(4f);
        }

        private FeedbackLogPlayer GetSelectedPlayer() {
            if (_selectedPlayerId == null) return null;

            for (int i = 0; i < _players.Count; i++) {
                if (_players[i].playerId == _selectedPlayerId) return _players[i];
            }

            return null;
        }

        private IReadOnlyList<FeedbackLogPlayer> GetSelectedPlayers() {
            var player = GetSelectedPlayer();
            if (player == null) return _players;

            _selectedPlayers.Clear();
            _selectedPlayers.Add(player);

            return _selectedPlayers;
        }

        private void DrawPlayer(FeedbackLogPlayer player) {
            string key = player.playerId;
            string title = $"{player.ShortId}  ·  {player.sessions.Count} sessions  ·  {player.entryCount} entries" +
                           (player.errorCount > 0 ? $"  ·  {player.errorCount} errors" : string.Empty) +
                           (string.IsNullOrEmpty(player.device) ? string.Empty : $"  ·  {player.device}");

            if (!DrawFoldout(key, title, FeedbackViewGui.BoldFoldout)) return;

            EditorGUI.indentLevel++;

            for (int i = 0; i < player.sessions.Count; i++) {
                DrawSession(player, player.sessions[i], number: i + 1);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawSession(FeedbackLogPlayer player, FeedbackLogSession session, int number) {
            var entries = GetVisibleEntries(session);
            if (entries.Count == 0) return;

            string key = $"{player.playerId}/{session.sessionId}";
            string title = $"#{number}  {session.startTime.ToLocalTime():yyyy-MM-dd HH:mm}  ·  " +
                           $"{FormatDuration(session.Duration)}  ·  {entries.Count} entries" +
                           (session.errorCount > 0 ? $"  ·  {session.errorCount} errors" : string.Empty) +
                           (string.IsNullOrEmpty(session.build) ? string.Empty : $"  ·  {session.build}") +
                           (string.IsNullOrEmpty(session.platform) ? string.Empty : $"  ·  {session.platform}");

            if (!DrawFoldout(key, title, EditorStyles.foldout)) return;

            EditorGUI.indentLevel++;

            for (int i = 0; i < entries.Count; i++) {
                DrawEntry(key, entries[i], i);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawEntry(string sessionKey, FeedbackLogEntry entry, int index) {
            var color = GUI.contentColor;
            if (entry.IsError) GUI.contentColor = new Color(1f, 0.5f, 0.5f);

            // A row is one line, so a message of several lines is folded: the first line names it,
            // the rest is shown when it is unfolded, together with the stack.
            string firstLine = GetFirstLine(entry.message);
            bool hasMore = !string.IsNullOrWhiteSpace(entry.stack) || firstLine.Length != entry.message?.Length;

            string title = $"{entry.time.ToLocalTime():HH:mm:ss}  {firstLine}";

            if (hasMore) {
                if (DrawFoldout($"{sessionKey}/{index}", title, EditorStyles.foldout)) {
                    GUI.contentColor = color;

                    EditorGUI.indentLevel++;
                    DrawEntryBody(entry);
                    EditorGUI.indentLevel--;
                }
            }
            else {
                FeedbackViewGui.DrawRow(title);
            }

            GUI.contentColor = color;
        }

        private static void DrawEntryBody(FeedbackLogEntry entry) {
            string text = string.IsNullOrWhiteSpace(entry.stack)
                ? entry.message
                : $"{entry.message}\n{entry.stack}";

            FeedbackViewGui.DrawText(text);

            if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(50f))) {
                EditorGUIUtility.systemCopyBuffer = text;
            }
        }

        private static string GetFirstLine(string message) {
            if (string.IsNullOrEmpty(message)) return string.Empty;

            int index = message.IndexOfAny(new[] { '\n', '\r' });
            return index < 0 ? message : message[..index];
        }



        private bool DrawFoldout(string key, string title, GUIStyle style) {
            bool expanded = _expanded.Contains(key);
            bool result = EditorGUILayout.Foldout(expanded, title, toggleOnLabelClick: true, style);

            if (result == expanded) return result;

            if (result) _expanded.Add(key);
            else _expanded.Remove(key);

            return result;
        }

        private List<FeedbackLogEntry> GetVisibleEntries(FeedbackLogSession session) {
            if (string.IsNullOrWhiteSpace(_search) && !_errorsOnly) return session.entries;

            var result = new List<FeedbackLogEntry>();

            for (int i = 0; i < session.entries.Count; i++) {
                var entry = session.entries[i];

                if (_errorsOnly && !entry.IsError) continue;
                if (!IsMatchingSearch(entry)) continue;

                result.Add(entry);
            }

            return result;
        }

        private bool IsMatchingSearch(FeedbackLogEntry entry) {
            if (string.IsNullOrWhiteSpace(_search)) return true;

            return entry.message?.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   entry.type?.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private int CountErrors() {
            int count = 0;

            for (int i = 0; i < _players.Count; i++) {
                count += _players[i].errorCount;
            }

            return count;
        }

        /// <summary>
        /// Keeps the entries as they were downloaded and rebuilds everything that depends on the filters.
        /// </summary>
        private void SetEntries(FeedbackLogEntry[] entries) {
            _entries = entries ?? Array.Empty<FeedbackLogEntry>();

            _platforms.Clear();
            _platformCounts.Clear();

            for (int i = 0; i < _entries.Length; i++) {
                string platform = GetPlatform(_entries[i]);

                if (!_platformCounts.ContainsKey(platform)) _platforms.Add(platform);

                _platformCounts[platform] = _platformCounts.GetValueOrDefault(platform) + 1;
            }

            _platforms.Sort(StringComparer.Ordinal);
            _disabledPlatforms.RemoveWhere(platform => !_platformCounts.ContainsKey(platform));

            RebuildPlayers();
        }

        private void RebuildPlayers() {
            var entries = new List<FeedbackLogEntry>(_entries.Length);

            for (int i = 0; i < _entries.Length; i++) {
                var entry = _entries[i];
                if (_disabledPlatforms.Contains(GetPlatform(entry))) continue;

                entries.Add(entry);
            }

            _visibleEntryCount = entries.Count;
            _players = FeedbackLogGrouping.Build(entries);

            // The selected player can be filtered out by the platforms.
            if (_selectedPlayerId != null && GetSelectedPlayer() == null) _selectedPlayerId = null;

            Repaint();
        }

        private static string GetPlatform(FeedbackLogEntry entry) {
            return string.IsNullOrWhiteSpace(entry.platform) ? UnknownPlatform : entry.platform;
        }

        private void DrawStatus() {
            EditorGUILayout.LabelField(_isDownloading ? "downloading..." : _status, EditorStyles.miniLabel);
        }

        private async UniTaskVoid Download() {
            if (GetSettingsError() is { } error) {
                _status = error;
                _showSettings = true;
                Repaint();
                return;
            }

            AsyncExt.RecreateCts(ref _cts);

            _isDownloading = true;
            Repaint();

            var result = await FeedbackLogLoader.Download(
                _config.AnalyzerCredentials.text,
                _config.AnalyzerSpreadsheetId,
                _config.AnalyzerSheetName,
                _cts.Token);

            _isDownloading = false;

            if (!result.ok) {
                _status = $"download failed: {result.message}";
                Repaint();
                return;
            }

            SetEntries(result.entries);

            _status = $"{_entries.Length} entries, {_players.Count} players, downloaded {DateTime.Now:HH:mm:ss}";

            Repaint();
        }

        private static string FormatDuration(TimeSpan duration) {
            return duration.TotalHours >= 1d
                ? $"{(int) duration.TotalHours} h {duration.Minutes} min"
                : duration.TotalMinutes >= 1d
                    ? $"{(int) duration.TotalMinutes} min"
                    : $"{(int) duration.TotalSeconds} s";
        }

        private void ReadPrefs() {
            string guid = EditorPrefs.GetString(ConfigKey, string.Empty);

            if (!string.IsNullOrEmpty(guid)) {
                _config = AssetDatabase.LoadAssetAtPath<FeedbackServiceConfig>(AssetDatabase.GUIDToAssetPath(guid));
            }

            // A project usually has a single config, so there is nothing to choose from on the first open.
            _config ??= AssetDatabase
                .FindAssets($"a:assets t:{nameof(FeedbackServiceConfig)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<FeedbackServiceConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault();

            _disabledPlatforms.Clear();

            foreach (string platform in EditorPrefs.GetString(DisabledPlatformsKey, string.Empty)
                         .Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                _disabledPlatforms.Add(platform);
            }
        }

        private void WritePrefs() {
            string guid = _config == null
                ? string.Empty
                : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_config));

            EditorPrefs.SetString(ConfigKey, guid);
            EditorPrefs.SetString(DisabledPlatformsKey, string.Join('|', _disabledPlatforms));
        }
    }

}
