using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Editor.Search {

    public abstract class AssetUsageSearchWindowBase : EditorWindow {

        [Serializable]
        protected sealed class ResultRow {
            public string Kind;
            public string AssetPath;
            public string ObjectPath;
            public string Info;
        }

        [Serializable]
        protected sealed class ResultGroup {
            public string Label;
            public List<ResultRow> Rows = new();
        }

        [Serializable]
        private sealed class SearchTab {
            public string Label = "New";
            public List<string> Targets = new();
            public List<ResultGroup> Groups = new();
            public bool HasResults;
        }

        [Serializable]
        private sealed class SavedFolders {
            public List<string> Paths = new();
        }

        [Serializable]
        private sealed class SavedSearchState {
            public bool SearchInAssets = true;
            public bool SearchInOpenedScenes;
            public bool SearchInAllScenes;
        }

        private sealed class SearchState : ScriptableObject {
            public List<DefaultAsset> searchFolders = new();
        }

        protected const string TargetsPropertyName = "targets";

        private const float ColDividerWidth = 5f;
        private const float MinColWidth = 60f;
        private const float TabMaxWidth = 400f;
        private const float TabMinWidth = 100f;

        private static readonly Color TopBgColor = new(0.314f, 0.314f, 0.314f, 1f);

        [SerializeField] private List<SearchTab> _tabs = new();
        [SerializeField] private int _activeTabIndex = -1;

        private SearchState _state;
        private SerializedObject _serializedState;
        private SerializedProperty _searchFoldersProperty;
        private SavedSearchState _options = new();

        private AssetFileScanner _scanner;
        private SearchTab _scanTab;
        private List<string> _scanTargets;

        private readonly Dictionary<SearchTab, (ScriptableObject state, SerializedObject serializedObject, SerializedProperty property)> _targetStates = new();
        private readonly Dictionary<string, Object> _objectCache = new();
        private Vector2 _scroll;
        private float _objectColWidth = 400f;
        private float _tableWidth;
        private bool _isDraggingCol;
        private float _dragStartMouseX;
        private float _dragStartColWidth;
        private float _topBgBottom;
        private GUIStyle _tabStyle;

        protected abstract string PrefsKeyPrefix { get; }
        protected abstract string TargetsLabel { get; }

        protected abstract ScriptableObject CreateTargetsState(IReadOnlyList<string> targets);
        protected abstract void ReadTargets(ScriptableObject state, List<string> targets);
        protected abstract string GetTargetLabel(string target);
        protected abstract void ConfigureRequest(AssetFileScanner.Request request);
        protected abstract void CollectResults(IReadOnlyList<AssetFileScanner.ScannedFile> files, IReadOnlyList<string> targets, List<ResultGroup> groups);

        private string SearchFoldersPrefsKey => $"{PrefsKeyPrefix}.SearchFolders.{Application.dataPath}";
        private string SearchStatePrefsKey => $"{PrefsKeyPrefix}.SearchState.{Application.dataPath}";

        protected void OpenTab(IReadOnlyList<string> targets) {
            var tab = new SearchTab();
            tab.Targets.AddRange(targets);
            tab.Label = BuildTabLabel(tab.Targets);

            _tabs.Add(tab);
            _activeTabIndex = _tabs.Count - 1;
            _scroll = Vector2.zero;

            GetTargetsState(tab).property.isExpanded = true;

            Repaint();
        }

        protected virtual void OnEnable() {
            minSize = new Vector2(520, 420);

            _state = CreateInstance<SearchState>();
            _state.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            _serializedState = new SerializedObject(_state);
            _searchFoldersProperty = _serializedState.FindProperty(nameof(SearchState.searchFolders));

            LoadSearchState();
            LoadSearchFolders();

            if (_activeTabIndex >= _tabs.Count) _activeTabIndex = _tabs.Count - 1;

            EditorApplication.update += OnEditorUpdate;
        }

        protected virtual void OnDisable() {
            EditorApplication.update -= OnEditorUpdate;

            CancelSearch();
            SaveSearchFolders();
            SaveSearchState();

            if (_state != null) DestroyImmediate(_state);
            _state = null;

            foreach (var targetState in _targetStates.Values) {
                if (targetState.state != null) DestroyImmediate(targetState.state);
            }

            _targetStates.Clear();
        }

        private void OnGUI() {
            if (_topBgBottom > 0f) EditorGUI.DrawRect(new Rect(0f, 0f, position.width, _topBgBottom), TopBgColor);

            DrawSearchOptions();
            DrawSearchFolders();
            EditorGUILayout.Space(4);

            if (Event.current.type == EventType.Repaint) {
                float bottom = GUILayoutUtility.GetLastRect().yMax;
                if (!Mathf.Approximately(_topBgBottom, bottom)) {
                    _topBgBottom = bottom;
                    Repaint();
                }
            }

            DrawTabBar();
            DrawActiveTab();
        }

        private void DrawSearchOptions() {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search In", EditorStyles.boldLabel, GUILayout.Width(64));
            _options.SearchInAssets = EditorGUILayout.ToggleLeft("Prefabs", _options.SearchInAssets, GUILayout.Width(66));
            GUILayout.Space(8);
            _options.SearchInOpenedScenes = EditorGUILayout.ToggleLeft("Opened Scenes", _options.SearchInOpenedScenes, GUILayout.Width(108));
            GUILayout.Space(8);
            _options.SearchInAllScenes = EditorGUILayout.ToggleLeft("All Scenes", _options.SearchInAllScenes, GUILayout.Width(88));
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck()) SaveSearchState();
        }

        private void DrawSearchFolders() {
            _serializedState.Update();
            EditorGUILayout.PropertyField(_searchFoldersProperty, includeChildren: true);
            if (_serializedState.ApplyModifiedProperties()) SaveSearchFolders();
        }

        private void DrawTabBar() {
            _tabStyle ??= new GUIStyle(EditorStyles.toolbarButton) { padding = { left = 8 } };

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            for (int i = 0; i < _tabs.Count; i++) {
                string fullLabel = _tabs[i].Label;
                var content = new GUIContent(TruncateLabelToFit(fullLabel, TabMaxWidth, _tabStyle), fullLabel);
                float width = Mathf.Clamp(_tabStyle.CalcSize(content).x, TabMinWidth, TabMaxWidth);

                bool isActive = i == _activeTabIndex;
                if (GUILayout.Toggle(isActive, content, _tabStyle, GUILayout.Width(width)) && !isActive) {
                    _activeTabIndex = i;
                    _scroll = Vector2.zero;
                }

                if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(18))) {
                    CloseTab(i);
                    GUIUtility.ExitGUI();
                }
            }

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(20))) {
                OpenTab(Array.Empty<string>());
                GUIUtility.ExitGUI();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawActiveTab() {
            if (_activeTabIndex < 0 || _activeTabIndex >= _tabs.Count) return;

            var tab = _tabs[_activeTabIndex];
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            var targetsState = GetTargetsState(tab);
            targetsState.serializedObject.Update();
            EditorGUILayout.PropertyField(targetsState.property, new GUIContent(TargetsLabel), includeChildren: true);

            if (targetsState.serializedObject.ApplyModifiedProperties()) {
                tab.Targets.Clear();
                ReadTargets(targetsState.state, tab.Targets);
                tab.Label = BuildTabLabel(tab.Targets);
            }

            EditorGUILayout.Space(4);
            DrawSearchButton(tab);
            EditorGUILayout.Space(4);

            if (tab.HasResults) DrawResults(tab);

            EditorGUILayout.EndScrollView();
        }

        private void DrawSearchButton(SearchTab tab) {
            if (_scanner == null) {
                using (new EditorGUI.DisabledScope(tab.Targets.Count == 0)) {
                    if (GUILayout.Button("Search", GUILayout.Height(28))) Search(tab);
                }

                return;
            }

            if (GUILayout.Button($"Searching {_scanner.ProcessedCount}/{_scanner.TotalCount}... Cancel", GUILayout.Height(28))) {
                CancelSearch();
            }
        }

        private void DrawResults(SearchTab tab) {
            int count = 0;
            for (int i = 0; i < tab.Groups.Count; i++) {
                count += tab.Groups[i].Rows.Count;
            }

            EditorGUILayout.LabelField($"Results: {count}", EditorStyles.boldLabel);
            DrawResultsHeader();

            for (int g = 0; g < tab.Groups.Count; g++) {
                var group = tab.Groups[g];

                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField($"{group.Label} ({group.Rows.Count})", EditorStyles.boldLabel);

                for (int i = 0; i < group.Rows.Count; i++) {
                    DrawResultRow(group.Rows[i], i);
                }
            }
        }

        private void DrawResultsHeader() {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            GetColumnRects(rect, out var objectRect, out var dividerRect, out var infoRect);

            EditorGUI.LabelField(objectRect, "Object", EditorStyles.boldLabel);
            EditorGUI.LabelField(infoRect, "Info", EditorStyles.boldLabel);

            HandleColumnDivider(dividerRect);
        }

        private void DrawResultRow(ResultRow row, int index) {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 2f);
            if (index % 2 == 0) EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.08f));

            GetColumnRects(rect, out var objectRect, out var dividerRect, out var infoRect);
            EditorGUI.DrawRect(new Rect(dividerRect.center.x - 0.5f, dividerRect.y, 1f, dividerRect.height), new Color(0.5f, 0.5f, 0.5f, 0.15f));

            objectRect.y += 1f;
            objectRect.height = EditorGUIUtility.singleLineHeight;
            infoRect.y += 1f;
            infoRect.height = EditorGUIUtility.singleLineHeight;

            var asset = LoadAsset(row.AssetPath);
            string assetName = asset != null ? asset.name : Path.GetFileNameWithoutExtension(row.AssetPath);
            string label = string.IsNullOrEmpty(row.ObjectPath) || row.ObjectPath == assetName ? assetName : $"{assetName} / {row.ObjectPath}";

            var content = asset != null
                ? new GUIContent(label, AssetPreview.GetMiniThumbnail(asset), $"{row.Kind}\n{row.AssetPath}\n{row.ObjectPath}")
                : new GUIContent(label, $"{row.Kind}\n{row.AssetPath}\n{row.ObjectPath}");

            if (GUI.Button(objectRect, content, EditorStyles.objectField)) OnRowClicked(row, asset);

            EditorGUI.SelectableLabel(infoRect, row.Info, EditorStyles.miniLabel);
        }

        private void OnRowClicked(ResultRow row, Object asset) {
            var target = FindSceneObject(row) ?? asset;
            if (target == null) return;

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);

            if (Event.current.clickCount >= 2 && asset != null) AssetDatabase.OpenAsset(asset);
        }

        private static GameObject FindSceneObject(ResultRow row) {
            if (row.Kind != AssetFileScanner.KindScene || string.IsNullOrEmpty(row.ObjectPath)) return null;

            var scene = SceneManager.GetSceneByPath(row.AssetPath);
            if (!scene.IsValid() || !scene.isLoaded) return null;

            string path = row.ObjectPath;
            int separator = path.IndexOf('/');
            string rootName = separator < 0 ? path : path[..separator];

            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++) {
                if (roots[i].name != rootName) continue;
                if (separator < 0) return roots[i];

                var child = roots[i].transform.Find(path[(separator + 1)..]);
                if (child != null) return child.gameObject;
            }

            return null;
        }

        private Object LoadAsset(string assetPath) {
            if (_objectCache.TryGetValue(assetPath, out var asset) && asset != null) return asset;

            asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            _objectCache[assetPath] = asset;
            return asset;
        }

        private void GetColumnRects(Rect rect, out Rect objectRect, out Rect dividerRect, out Rect infoRect) {
            _tableWidth = rect.width;

            float objectWidth = Mathf.Clamp(_objectColWidth, MinColWidth, Mathf.Max(MinColWidth, rect.width - ColDividerWidth - MinColWidth));

            objectRect = new Rect(rect.x, rect.y, objectWidth, rect.height);
            dividerRect = new Rect(objectRect.xMax, rect.y, ColDividerWidth, rect.height);
            infoRect = new Rect(dividerRect.xMax, rect.y, rect.width - objectWidth - ColDividerWidth, rect.height);
        }

        private void HandleColumnDivider(Rect rect) {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            EditorGUI.DrawRect(new Rect(rect.center.x - 0.5f, rect.y, 1f, rect.height), new Color(0.5f, 0.5f, 0.5f, 0.5f));

            var e = Event.current;

            switch (e.type) {
                case EventType.MouseDown when rect.Contains(e.mousePosition):
                    _isDraggingCol = true;
                    _dragStartMouseX = e.mousePosition.x;
                    _dragStartColWidth = _objectColWidth;
                    e.Use();
                    break;

                case EventType.MouseDrag when _isDraggingCol:
                    _objectColWidth = Mathf.Clamp(_dragStartColWidth + e.mousePosition.x - _dragStartMouseX, MinColWidth, _tableWidth - MinColWidth - ColDividerWidth);
                    Repaint();
                    e.Use();
                    break;

                case EventType.MouseUp when _isDraggingCol:
                    _isDraggingCol = false;
                    e.Use();
                    break;
            }
        }

        private void Search(SearchTab tab) {
            if (_scanner != null) return;

            string[] searchFolders = GetValidSearchFolderPaths();
            SaveSearchFolders();

            bool searchPrefabs = _options.SearchInAssets;
            bool searchAllScenes = _options.SearchInAllScenes;
            bool searchOpenedScenes = _options.SearchInOpenedScenes && !searchAllScenes;

            if (searchFolders.Length == 0 && !searchOpenedScenes) return;
            if ((searchOpenedScenes || searchAllScenes) && !AssetFileScanner.SaveOpenedScenesIfUserWantsTo()) return;

            var request = new AssetFileScanner.Request();
            request.AddFiles(searchFolders, assets: false, prefabs: searchPrefabs, scenes: searchAllScenes);
            if (searchOpenedScenes) request.AddOpenedScenes();

            ConfigureRequest(request);

            _scanTab = tab;
            _scanTargets = new List<string>(tab.Targets);
            _scanner = AssetFileScanner.Start(request);
        }

        private void CancelSearch() {
            _scanner?.Cancel();
            _scanner = null;
            _scanTab = null;
            _scanTargets = null;
        }

        private void OnEditorUpdate() {
            if (_scanner == null) return;

            if (_scanner.IsDone) FinishSearch();
            Repaint();
        }

        private void FinishSearch() {
            var scanner = _scanner;
            var tab = _scanTab;
            var targets = _scanTargets;

            _scanner = null;
            _scanTab = null;
            _scanTargets = null;

            scanner.Complete();
            if (scanner.IsCancelled || !_tabs.Contains(tab)) return;

            tab.Groups.Clear();
            CollectResults(scanner.Files, targets, tab.Groups);

            for (int i = 0; i < tab.Groups.Count; i++) {
                tab.Groups[i].Rows.Sort((a, b) => {
                    int result = string.CompareOrdinal(a.AssetPath, b.AssetPath);
                    return result != 0 ? result : string.CompareOrdinal(a.ObjectPath, b.ObjectPath);
                });
            }

            tab.HasResults = true;
            _objectCache.Clear();
        }

        private (ScriptableObject state, SerializedObject serializedObject, SerializedProperty property) GetTargetsState(SearchTab tab) {
            if (_targetStates.TryGetValue(tab, out var targetsState) && targetsState.state != null) return targetsState;

            var state = CreateTargetsState(tab.Targets);
            state.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            var serializedObject = new SerializedObject(state);
            targetsState = (state, serializedObject, serializedObject.FindProperty(TargetsPropertyName));
            _targetStates[tab] = targetsState;

            return targetsState;
        }

        private void CloseTab(int index) {
            var tab = _tabs[index];
            if (_scanTab == tab) CancelSearch();

            if (_targetStates.Remove(tab, out var targetsState) && targetsState.state != null) DestroyImmediate(targetsState.state);

            _tabs.RemoveAt(index);

            if (_activeTabIndex >= _tabs.Count) _activeTabIndex = _tabs.Count - 1;
            else if (_activeTabIndex > index) _activeTabIndex--;
        }

        private string BuildTabLabel(List<string> targets) {
            if (targets.Count == 0) return "New";

            string first = GetTargetLabel(targets[0]);
            return targets.Count == 1 ? first : $"{first} [+{targets.Count - 1}]";
        }

        private static string TruncateLabelToFit(string label, float maxWidth, GUIStyle style) {
            if (style.CalcSize(new GUIContent(label)).x <= maxWidth) return label;

            const string ellipsis = "…";
            int lo = 0;
            int hi = label.Length - 1;

            while (lo < hi) {
                int mid = (lo + hi + 1) / 2;
                if (style.CalcSize(new GUIContent(label[..mid] + ellipsis)).x <= maxWidth) lo = mid;
                else hi = mid - 1;
            }

            return label[..lo] + ellipsis;
        }

        private string[] GetValidSearchFolderPaths() {
            var paths = new List<string>();

            for (int i = 0; i < _state.searchFolders.Count; i++) {
                string path = GetFolderPath(_state.searchFolders[i]);
                if (!string.IsNullOrEmpty(path) && !paths.Contains(path)) paths.Add(path);
            }

            return paths.ToArray();
        }

        private static string GetFolderPath(DefaultAsset folder) {
            if (folder == null) return null;

            string path = AssetDatabase.GetAssetPath(folder);
            return !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path) ? path : null;
        }

        private void SaveSearchState() {
            EditorPrefs.SetString(SearchStatePrefsKey, JsonUtility.ToJson(_options));
        }

        private void LoadSearchState() {
            string json = EditorPrefs.GetString(SearchStatePrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return;

            try {
                _options = JsonUtility.FromJson<SavedSearchState>(json) ?? new SavedSearchState();
            }
            catch {
                _options = new SavedSearchState();
            }
        }

        private void SaveSearchFolders() {
            var saved = new SavedFolders();
            saved.Paths.AddRange(GetValidSearchFolderPaths());

            EditorPrefs.SetString(SearchFoldersPrefsKey, JsonUtility.ToJson(saved));
        }

        private void LoadSearchFolders() {
            _state.searchFolders.Clear();

            string json = EditorPrefs.GetString(SearchFoldersPrefsKey, string.Empty);
            SavedFolders saved = null;

            if (!string.IsNullOrEmpty(json)) {
                try {
                    saved = JsonUtility.FromJson<SavedFolders>(json);
                }
                catch {
                    saved = null;
                }
            }

            if (saved?.Paths != null) {
                for (int i = 0; i < saved.Paths.Count; i++) {
                    var folder = AssetDatabase.IsValidFolder(saved.Paths[i]) ? AssetDatabase.LoadAssetAtPath<DefaultAsset>(saved.Paths[i]) : null;
                    if (folder != null && !_state.searchFolders.Contains(folder)) _state.searchFolders.Add(folder);
                }
            }

            if (_state.searchFolders.Count > 0) return;

            var assetsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets");
            if (assetsFolder != null) _state.searchFolders.Add(assetsFolder);
        }
    }

}
