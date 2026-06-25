using System;
using System.Collections.Generic;
using MisterGames.Common.Editor.Menu;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MisterGames.Scenario.Editor.Events {

    public sealed class EventReferenceSearchWindow : EditorWindow {

        private sealed class SearchState : ScriptableObject
        {
            public List<DefaultAsset> searchFolders = new();
            public bool searchInAssets = true;
            public bool searchInOpenedScenes = false;
            public bool searchInAllScenes = false;
        }

        private sealed class TabTargetsState : ScriptableObject {
            public List<EventReference> targets = new();
        }

        private sealed class SingleEventReferenceState : ScriptableObject
        {
            public EventReference eventReference;
        }

        [Serializable]
        private sealed class SearchTabGroup
        {
            public string DomainPath;
            public int Id;
            public string Label;
            public int ResultStartIndex;
            public int ResultCount;
        }

        [Serializable]
        private sealed class SearchTab
        {
            public string Label;
            public List<SearchTabGroup> Groups = new();
            public List<SavedUsage> Results = new();
            public List<string> TargetDomainPaths = new();
            public List<int> TargetIds = new();
        }

        private sealed class TabRuntime
        {
            public readonly List<SingleEventReferenceState> GroupHeaderStates = new();
            public readonly List<SerializedObject> GroupHeaderSerializedObjects = new();
            public readonly List<SerializedProperty> GroupHeaderProperties = new();
            public readonly List<SerializedProperty> GroupDomainProperties = new();
            public readonly List<SerializedReferenceUsageFinder.Usage> Results = new();
            public Vector2 Scroll;
            public readonly Dictionary<string, Object> ResultObjectCache = new();

            public TabTargetsState TargetsState;
            public SerializedObject TargetsSerializedObject;
            public SerializedProperty TargetsProperty;
        }

        [Serializable]
        private sealed class SavedFolders
        {
            public List<string> Paths = new();
        }

        [Serializable]
        private sealed class SavedSearchState
        {
            public bool SearchInAssets = true;
            public bool SearchInOpenedScenes = false;
            public bool SearchInAllScenes = false;
        }

        [Serializable]
        private sealed class SavedUsage {
            public string Kind;
            public string AssetPath;
            public string ObjectPath;
            public string OwnerType;
            public string PropertyPath;
        }

        private const string SearchFoldersPrefsKeyPrefix = "EventReferenceSearchWindow.SearchFolders.";
        private static string SearchFoldersPrefsKey => SearchFoldersPrefsKeyPrefix + Application.dataPath;

        private const string SearchStatePrefsKeyPrefix = "EventReferenceSearchWindow.SearchState.";
        private static string SearchStatePrefsKey => SearchStatePrefsKeyPrefix + Application.dataPath;

        private SearchState _state;
        private SerializedObject _serializedState;
        private SerializedProperty _searchFoldersProperty;
        private SerializedProperty _searchInAssetsProperty;
        private SerializedProperty _searchInOpenedScenesProperty;
        private SerializedProperty _searchInAllScenesProperty;

        private EventReference[] _requestedSearch;

        private float _assetColWidth = 230f;
        private float _scriptColWidth = 170f;
        private float _tableWidth;
        private int _draggingCol = -1;
        private float _dragStartMouseX;
        private float _dragStartColWidth;
        private GUIStyle _tabStyle;

        private const float ColDividerWidth = 5f;
        private const float MinColWidth = 60f;
        private const float TabMaxWidth = 400f;
        private const float TabMinWidth = 100f;

        [SerializeField] private List<SearchTab> _tabs = new();
        [SerializeField] private int _activeTabIndex = -1;
        private readonly List<TabRuntime> _tabRuntimes = new();

        private static Dictionary<string, MonoScript> _monoScriptByTypeFullName;

        [MenuItem("MisterGames/Tools/Event Reference Search")]
        public static void Open() {
            var window = GetWindow<EventReferenceSearchWindow>("Event Reference Search");
            SetupSize(window);
        }

        public static void SearchEventReference(EventReference eventReference) {
            var window = GetWindow<EventReferenceSearchWindow>("Event Reference Search");
            SetupSize(window);
            window._requestedSearch = new[] { eventReference };
            window.ApplyRequestedSearchToActiveTab();
        }

        public static void SearchEventReferences(EventReference[] eventReferences) {
            var window = GetWindow<EventReferenceSearchWindow>("Event Reference Search");
            SetupSize(window);
            window._requestedSearch = eventReferences;
            window.ApplyRequestedSearchToActiveTab();
        }

        private static void SetupSize(EventReferenceSearchWindow window) {
            window.minSize = new Vector2(520, 420);
            window.maxSize = new Vector2(3840, 2160);
        }

        private void OnEnable() {
            _state = CreateInstance<SearchState>();
            _state.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            _serializedState = new SerializedObject(_state);
            _searchFoldersProperty = _serializedState.FindProperty(nameof(SearchState.searchFolders));
            _searchInAssetsProperty = _serializedState.FindProperty(nameof(SearchState.searchInAssets));
            _searchInOpenedScenesProperty = _serializedState.FindProperty(nameof(SearchState.searchInOpenedScenes));
            _searchInAllScenesProperty = _serializedState.FindProperty(nameof(SearchState.searchInAllScenes));

            LoadSearchState();

            // Rebuild tab runtimes from serialized tabs
            _tabRuntimes.Clear();
            foreach (var tab in _tabs) {
                var runtime = CreateTabRuntime();

                for (int i = 0; i < tab.TargetDomainPaths.Count && i < tab.TargetIds.Count; i++) {
                    var domain = AssetDatabase.LoadAssetAtPath<EventDomain>(tab.TargetDomainPaths[i]);
                    if (domain != null) runtime.TargetsState.targets.Add(new EventReference(domain, tab.TargetIds[i]));
                }

                foreach (var group in tab.Groups) {
                    var lib = AssetDatabase.LoadAssetAtPath<EventDomain>(group.DomainPath);
                    AddGroupHeaderToRuntime(runtime, lib, group.Id);
                }
                foreach (var s in tab.Results) {
                    runtime.Results.Add(new SerializedReferenceUsageFinder.Usage(
                        s.Kind, s.AssetPath, s.ObjectPath, s.OwnerType, s.PropertyPath));
                }
                _tabRuntimes.Add(runtime);
            }

            if (_activeTabIndex >= _tabs.Count) _activeTabIndex = _tabs.Count - 1;

            ApplyRequestedSearchToActiveTab();

            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;

            LoadSearchFolders();

            if (_state.searchFolders.Count == 0) {
                var assetsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets");
                if (assetsFolder != null) {
                    _state.searchFolders.Add(assetsFolder);
                    SaveSearchFolders();
                }
            }
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneClosed -= OnSceneClosed;

            SaveSearchFolders();
            SaveSearchState();

            if (_state != null)
            {
                DestroyImmediate(_state);
                _state = null;
            }

            foreach (var runtime in _tabRuntimes) {
                foreach (var state in runtime.GroupHeaderStates) {
                    if (state != null) DestroyImmediate(state);
                }
                if (runtime.TargetsState != null) DestroyImmediate(runtime.TargetsState);
            }
            _tabRuntimes.Clear();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode) => InvalidateSceneCache();
        private void OnSceneClosed(Scene scene) => InvalidateSceneCache();

        private void InvalidateSceneCache() {
            foreach (var runtime in _tabRuntimes) {
                var toRemove = new List<string>();
                foreach (var key in runtime.ResultObjectCache.Keys) {
                    if (key.Contains(".unity")) toRemove.Add(key);
                }
                foreach (var key in toRemove) runtime.ResultObjectCache.Remove(key);
            }
            Repaint();
        }

        private static readonly Color TopBgColor = new Color(0.314f, 0.314f, 0.314f, 1f);
        private float _topBgBottom;

        private void OnGUI() {
            if (_topBgBottom > 0f)
                EditorGUI.DrawRect(new Rect(0f, 0f, position.width, _topBgBottom), TopBgColor);

            DrawSearchOptions();
            DrawSearchFolders();
            EditorGUILayout.Space(4);

            if (Event.current.type == EventType.Repaint) {
                float newBottom = GUILayoutUtility.GetLastRect().yMax;
                if (_topBgBottom != newBottom) {
                    _topBgBottom = newBottom;
                    Repaint();
                }
            }

            DrawResults();
        }

        private void DrawSearchOptions() {
            _serializedState.Update();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search In", EditorStyles.boldLabel, GUILayout.Width(64));
            _searchInAssetsProperty.boolValue = EditorGUILayout.ToggleLeft("Assets", _searchInAssetsProperty.boolValue, GUILayout.Width(62));
            GUILayout.Space(8);
            _searchInOpenedScenesProperty.boolValue = EditorGUILayout.ToggleLeft("Opened Scenes", _searchInOpenedScenesProperty.boolValue, GUILayout.Width(108));
            GUILayout.Space(8);
            _searchInAllScenesProperty.boolValue = EditorGUILayout.ToggleLeft("All Scenes", _searchInAllScenesProperty.boolValue, GUILayout.Width(88));
            EditorGUILayout.EndHorizontal();
            if (_serializedState.ApplyModifiedProperties()) SaveSearchState();
        }

        private void DrawSearchFolders() {
            _serializedState.Update();
            EditorGUILayout.PropertyField(_searchFoldersProperty, includeChildren: true);
            if (_serializedState.ApplyModifiedProperties()) {
                SaveSearchFolders();
            }
        }

        private void DrawSearchButton() {
            if (GUILayout.Button("Search", GUILayout.Height(28))) Search();
        }

        private void DrawResults() {
            DrawTabBar();

            if (_activeTabIndex < 0 || _activeTabIndex >= _tabRuntimes.Count) return;

            var rt = _tabRuntimes[_activeTabIndex];
            var tab = _tabs[_activeTabIndex];

            rt.Scroll = EditorGUILayout.BeginScrollView(rt.Scroll);

            DrawActiveTabTargets(rt);

            EditorGUILayout.Space(4);
            DrawSearchButton();
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField($"Results: {rt.Results.Count}", EditorStyles.boldLabel);
            DrawResultsHeader();

            for (int g = 0; g < tab.Groups.Count; g++) {
                var group = tab.Groups[g];
                DrawGroupHeader(rt, g);
                for (int i = group.ResultStartIndex; i < group.ResultStartIndex + group.ResultCount; i++) {
                    DrawResultRow(rt, rt.Results[i], i - group.ResultStartIndex);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawActiveTabTargets(TabRuntime rt) {
            if (rt.TargetsSerializedObject == null) return;
            rt.TargetsSerializedObject.Update();
            EditorGUILayout.PropertyField(rt.TargetsProperty, includeChildren: true);
            if (rt.TargetsSerializedObject.ApplyModifiedProperties()) {
                SyncTabTargets(_activeTabIndex);
                if (_activeTabIndex >= 0 && _activeTabIndex < _tabs.Count) {
                    _tabs[_activeTabIndex].Label = BuildTabLabel(rt.TargetsState.targets);
                }
            }
        }

        private static void DrawGroupHeader(TabRuntime rt, int groupIndex) {
            if (groupIndex >= rt.GroupHeaderSerializedObjects.Count) return;

            rt.GroupHeaderSerializedObjects[groupIndex].Update();

            var totalRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing * 3f);

            const float gap = 4f;
            float half = (totalRect.width - gap) * 0.5f;
            var labelValueRect = new Rect(totalRect.x, totalRect.y, half, totalRect.height);

            using (new EditorGUI.DisabledScope(true)) {
                EditorGUI.PropertyField(labelValueRect, rt.GroupHeaderProperties[groupIndex], new GUIContent("Search value"));
            }
        }

        private void DrawTabBar() {
            if (_tabStyle == null) {
                _tabStyle = new GUIStyle(EditorStyles.toolbarButton);
                _tabStyle.padding.left = 8;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            for (int i = 0; i < _tabs.Count; i++) {
                bool isActive = i == _activeTabIndex;
                string fullLabel = _tabs[i].Label;

                string display = TruncateLabelToFit(fullLabel, TabMaxWidth, _tabStyle);
                var content = new GUIContent(display, fullLabel);
                float tabW = Mathf.Clamp(_tabStyle.CalcSize(content).x, TabMinWidth, TabMaxWidth);

                bool clicked = GUILayout.Toggle(isActive, content, _tabStyle, GUILayout.Width(tabW));
                if (clicked && !isActive) _activeTabIndex = i;

                if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(18))) {
                    CloseTab(i);
                    GUIUtility.ExitGUI();
                    break;
                }
            }

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(20))) {
                _tabs.Add(new SearchTab { Label = "New" });
                _tabRuntimes.Add(CreateTabRuntime());
                _activeTabIndex = _tabs.Count - 1;
                SaveSearchState();
                GUIUtility.ExitGUI();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private static string TruncateLabelToFit(string label, float maxWidth, GUIStyle style) {
            if (style.CalcSize(new GUIContent(label)).x <= maxWidth) return label;

            const string ellipsis = "…";
            int lo = 0, hi = label.Length - 1;
            while (lo < hi) {
                int mid = (lo + hi + 1) / 2;
                if (style.CalcSize(new GUIContent(label.Substring(0, mid) + ellipsis)).x <= maxWidth)
                    lo = mid;
                else
                    hi = mid - 1;
            }

            string truncated = label.Substring(0, lo) + ellipsis;
            return style.CalcSize(new GUIContent(truncated)).x <= maxWidth ? truncated : ellipsis;
        }

        private void DrawResultsHeader() {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            GetColumnRects(rect, out var assetRect, out var div0, out var scriptRect, out var div1, out var propertyRect);

            EditorGUI.LabelField(assetRect, "Object", EditorStyles.boldLabel);
            EditorGUI.LabelField(scriptRect, "Script", EditorStyles.boldLabel);
            EditorGUI.LabelField(propertyRect, "Property", EditorStyles.boldLabel);

            HandleColumnDivider(div0, 0);
            HandleColumnDivider(div1, 1);
        }

        private void DrawResultRow(
            TabRuntime tabRuntime,
            SerializedReferenceUsageFinder.Usage usage,
            int index) {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 2f);

            if (index % 2 == 0)
                EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.08f));

            GetColumnRects(rect, out var assetRect, out var div0, out var scriptRect, out var div1, out var propertyRect);

            EditorGUI.DrawRect(new Rect(div0.x + div0.width * 0.5f - 0.5f, div0.y, 1f, div0.height), new Color(0.5f, 0.5f, 0.5f, 0.15f));
            EditorGUI.DrawRect(new Rect(div1.x + div1.width * 0.5f - 0.5f, div1.y, 1f, div1.height), new Color(0.5f, 0.5f, 0.5f, 0.15f));

            float yOffset = 1f;
            assetRect = new Rect(assetRect.x, assetRect.y + yOffset, assetRect.width, EditorGUIUtility.singleLineHeight);
            scriptRect = new Rect(scriptRect.x, scriptRect.y + yOffset, scriptRect.width, EditorGUIUtility.singleLineHeight);
            propertyRect = new Rect(propertyRect.x, propertyRect.y + yOffset, propertyRect.width, EditorGUIUtility.singleLineHeight);

            Object resultObject = ResolveResultObject(usage, tabRuntime.ResultObjectCache);
            Object pingObject = IsSceneResult(usage) ? (ResolveScenePingObject(usage, tabRuntime.ResultObjectCache) ?? resultObject) : resultObject;
            MonoScript ownerScript = ResolveOwnerScript(usage.OwnerType);

            string assetLabel = BuildAssetObjectLabel(usage, resultObject);
            string assetTooltip =
                $"{usage.Kind}\n{usage.AssetPath}\n{usage.ObjectPath}";

            DrawPingableObjectCell(
                assetRect,
                resultObject,
                pingObject,
                assetLabel,
                assetTooltip);

            string scriptLabel = ownerScript != null
                ? ownerScript.name
                : GetShortTypeName(usage.OwnerType);

            DrawPingableObjectCell(
                scriptRect,
                ownerScript,
                ownerScript,
                scriptLabel,
                usage.OwnerType);

            EditorGUI.SelectableLabel(
                propertyRect,
                FormatPropertyPath(usage.PropertyPath),
                EditorStyles.miniLabel);
        }

        private void GetColumnRects(Rect rect, out Rect assetRect, out Rect div0, out Rect scriptRect, out Rect div1, out Rect propertyRect) {
            _tableWidth = rect.width;

            float maxTotal = rect.width - ColDividerWidth * 2f - MinColWidth;
            float assetW = _assetColWidth;
            float scriptW = _scriptColWidth;
            if (assetW + scriptW > maxTotal) {
                float ratio = assetW / (assetW + scriptW);
                assetW = maxTotal * ratio;
                scriptW = maxTotal * (1f - ratio);
            }

            float propertyWidth = rect.width - assetW - scriptW - ColDividerWidth * 2f;

            assetRect = new Rect(rect.x, rect.y, assetW, rect.height);
            div0 = new Rect(assetRect.xMax, rect.y, ColDividerWidth, rect.height);
            scriptRect = new Rect(div0.xMax, rect.y, scriptW, rect.height);
            div1 = new Rect(scriptRect.xMax, rect.y, ColDividerWidth, rect.height);
            propertyRect = new Rect(div1.xMax, rect.y, propertyWidth, rect.height);
        }

        private void HandleColumnDivider(Rect rect, int colIndex) {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);

            EditorGUI.DrawRect(
                new Rect(rect.x + rect.width * 0.5f - 0.5f, rect.y, 1f, rect.height),
                new Color(0.5f, 0.5f, 0.5f, 0.5f));

            var e = Event.current;

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition)) {
                _draggingCol = colIndex;
                _dragStartMouseX = e.mousePosition.x;
                _dragStartColWidth = colIndex == 0 ? _assetColWidth : _scriptColWidth;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _draggingCol == colIndex) {
                float delta = e.mousePosition.x - _dragStartMouseX;
                float newWidth = _dragStartColWidth + delta;

                if (colIndex == 0) {
                    float maxW = _tableWidth - _scriptColWidth - MinColWidth - ColDividerWidth * 2f;
                    _assetColWidth = Mathf.Clamp(newWidth, MinColWidth, maxW);
                }
                else {
                    float maxW = _tableWidth - _assetColWidth - MinColWidth - ColDividerWidth * 2f;
                    _scriptColWidth = Mathf.Clamp(newWidth, MinColWidth, maxW);
                }

                Repaint();
                e.Use();
            }
            else if (e.type == EventType.MouseUp && _draggingCol == colIndex) {
                _draggingCol = -1;
                e.Use();
            }
        }

        private static Object ResolveResultObject(
            SerializedReferenceUsageFinder.Usage usage,
            Dictionary<string, Object> cache) {
            string cacheKey =
                $"{usage.Kind}|{usage.AssetPath}|{usage.ObjectPath}|{usage.OwnerType}";

            if (cache.TryGetValue(cacheKey, out var cached))
                return cached;

            Object result = null;

            if (usage.Kind == "Asset") {
                var objects = AssetDatabase.LoadAllAssetsAtPath(usage.AssetPath);

                foreach (var obj in objects) {
                    if (obj == null)
                        continue;

                    if (obj.name == usage.ObjectPath &&
                        obj.GetType().FullName == usage.OwnerType) {
                        result = obj;
                        break;
                    }
                }

                if (result == null)
                    result = AssetDatabase.LoadMainAssetAtPath(usage.AssetPath);
            }
            else {
                result = AssetDatabase.LoadAssetAtPath<Object>(usage.AssetPath);
            }

            cache[cacheKey] = result;
            return result;
        }

        private static bool IsSceneResult(SerializedReferenceUsageFinder.Usage usage) =>
            usage.AssetPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);

        private static Object ResolveScenePingObject(
            SerializedReferenceUsageFinder.Usage usage,
            Dictionary<string, Object> cache) {
            string cacheKey = $"ping|{usage.AssetPath}|{usage.ObjectPath}|{usage.OwnerType}";

            if (cache.TryGetValue(cacheKey, out var cached))
                return cached;

            Object result = null;
            var scene = SceneManager.GetSceneByPath(usage.AssetPath);

            if (scene.IsValid() && scene.isLoaded) {
                var go = FindGameObjectInScene(scene, usage.ObjectPath);
                if (go != null) {
                    var type = FindTypeByName(usage.OwnerType);
                    result = type != null ? (Object) go.GetComponent(type) ?? go : go;
                }
            }

            cache[cacheKey] = result;
            return result;
        }

        private static GameObject FindGameObjectInScene(Scene scene, string objectPath) {
            if (string.IsNullOrEmpty(objectPath)) return null;

            int sep = objectPath.IndexOf('/');
            string rootName = sep < 0 ? objectPath : objectPath.Substring(0, sep);

            GameObject root = null;
            foreach (var go in scene.GetRootGameObjects()) {
                if (go.name == rootName) { root = go; break; }
            }

            if (root == null || sep < 0) return root;

            string relativePath = objectPath.Substring(sep + 1);
            var child = root.transform.Find(relativePath);
            return child != null ? child.gameObject : null;
        }

        private static Type FindTypeByName(string fullTypeName) {
            if (string.IsNullOrEmpty(fullTypeName)) return null;
            var type = Type.GetType(fullTypeName);
            if (type != null) return type;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                type = asm.GetType(fullTypeName);
                if (type != null) return type;
            }
            return null;
        }

        private static MonoScript ResolveOwnerScript(string ownerTypeFullName) {
            if (string.IsNullOrEmpty(ownerTypeFullName))
                return null;

            EnsureMonoScriptCache();

            _monoScriptByTypeFullName.TryGetValue(ownerTypeFullName, out var script);
            return script;
        }

        private static void EnsureMonoScriptCache() {
            if (_monoScriptByTypeFullName != null)
                return;

            _monoScriptByTypeFullName = new Dictionary<string, MonoScript>();

            string[] guids = AssetDatabase.FindAssets("t:MonoScript");

            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script == null)
                    continue;

                var type = script.GetClass();

                if (type == null)
                    continue;

                string fullName = type.FullName;

                if (string.IsNullOrEmpty(fullName))
                    continue;

                if (!_monoScriptByTypeFullName.ContainsKey(fullName))
                    _monoScriptByTypeFullName.Add(fullName, script);
            }
        }

        private static string BuildAssetObjectLabel(
            SerializedReferenceUsageFinder.Usage usage,
            Object resultObject) {
            string assetName = resultObject != null
                ? resultObject.name
                : System.IO.Path.GetFileNameWithoutExtension(usage.AssetPath);

            if (string.IsNullOrEmpty(usage.ObjectPath) ||
                usage.ObjectPath == assetName)
                return assetName;

            return $"{assetName} / {usage.ObjectPath}";
        }

        private static string FormatPropertyPath(string path) {
            if (string.IsNullOrEmpty(path)) return path;
            path = path.Replace(".Array.data[", "[");
            if (path.EndsWith($".{EventReferencePropertyDrawer.EventDomainPropertyPath}"))
                path = path[..^$".{EventReferencePropertyDrawer.EventDomainPropertyPath}".Length];
            return path;
        }

        private static string GetShortTypeName(string fullTypeName) {
            if (string.IsNullOrEmpty(fullTypeName))
                return "<unknown>";

            int lastDot = fullTypeName.LastIndexOf('.');

            if (lastDot < 0)
                return fullTypeName;

            return fullTypeName.Substring(lastDot + 1);
        }

        private static void DrawPingableObjectCell(
            Rect rect,
            Object iconObj,
            Object pingObj,
            string label,
            string tooltip) {
            GUIContent content = iconObj != null
                ? new GUIContent(label, AssetPreview.GetMiniThumbnail(iconObj), tooltip)
                : new GUIContent(label, tooltip);

            bool clicked = GUI.Button(rect, content, EditorStyles.objectField);

            if (!clicked || pingObj == null)
                return;

            Selection.activeObject = pingObj;
            EditorGUIUtility.PingObject(pingObj);

            if (Event.current.clickCount >= 2 && AssetDatabase.Contains(pingObj))
                AssetDatabase.OpenAsset(pingObj);
        }

        private void Search() {
            if (_activeTabIndex < 0 || _activeTabIndex >= _tabs.Count) {
                _tabs.Add(new SearchTab { Label = "New" });
                _tabRuntimes.Add(CreateTabRuntime());
                _activeTabIndex = _tabs.Count - 1;
            }

            var activeRt = _tabRuntimes[_activeTabIndex];
            var activeTab = _tabs[_activeTabIndex];
            var targets = activeRt.TargetsState.targets;

            string[] searchFolders = GetValidSearchFolderPaths();
            SaveSearchFolders();

            bool searchAssets = _state.searchInAssets;
            bool searchAllScenes = _state.searchInAllScenes;
            bool searchOpenedScenes = _state.searchInOpenedScenes && !searchAllScenes;

            if (searchFolders.Length == 0 && !searchOpenedScenes && !searchAllScenes) return;

            var libraryGroups = new Dictionary<EventDomain, HashSet<int>>();
            foreach (var entry in targets) {
                if (entry.EventDomain == null) continue;
                if (!libraryGroups.TryGetValue(entry.EventDomain, out var ids)) {
                    ids = new HashSet<int>();
                    libraryGroups[entry.EventDomain] = ids;
                }
                ids.Add(entry.EventId);
            }

            var matchedBy = new Dictionary<string, (string libPath, int id)>();
            var allResults = new List<SerializedReferenceUsageFinder.Usage>();
            var seen = new HashSet<string>();

            foreach (var (lib, ids) in libraryGroups) {
                string libPath = AssetDatabase.GetAssetPath(lib);

                bool Predicate(SerializedReferenceUsageFinder.UsageCandidate<EventDomain> u) {
                    if (!IsEventReference(u, lib, ids, out int id)) return false;
                    string k = $"{u.Kind}|{u.AssetPath}|{u.ObjectPath}|{u.OwnerType}|{u.PropertyPath}";
                    if (!matchedBy.ContainsKey(k)) matchedBy[k] = (libPath, id);
                    return true;
                }

                if (searchAssets)
                    AddResultsUnique(SerializedReferenceUsageFinder.FindUsagesInAssetsAndPrefabs(
                        lib, searchFolders, new[] { typeof(ScriptableObject) }, Predicate), allResults, seen);
                if (searchAllScenes)
                    AddResultsUnique(SerializedReferenceUsageFinder.FindUsagesInAllScenes(
                        lib, searchFolders, predicate: Predicate), allResults, seen);
                else if (searchOpenedScenes)
                    AddResultsUnique(SerializedReferenceUsageFinder.FindUsagesInOpenedScenes(
                        lib, predicate: Predicate), allResults, seen);
            }

            var resultsByTarget = new Dictionary<(string, int), List<SerializedReferenceUsageFinder.Usage>>();
            foreach (var r in allResults) {
                string k = $"{r.Kind}|{r.AssetPath}|{r.ObjectPath}|{r.OwnerType}|{r.PropertyPath}";
                if (!matchedBy.TryGetValue(k, out var pair)) continue;
                if (!resultsByTarget.TryGetValue(pair, out var list)) {
                    list = new List<SerializedReferenceUsageFinder.Usage>();
                    resultsByTarget[pair] = list;
                }
                list.Add(r);
            }

            var orderedResults = new List<SerializedReferenceUsageFinder.Usage>();
            var tabGroups = new List<SearchTabGroup>();
            var domainIds = new List<(EventDomain domain, int id)>();
            var seenPairs = new HashSet<(string, int)>();

            foreach (var target in targets) {
                if (target.EventDomain == null) continue;
                string libPath = AssetDatabase.GetAssetPath(target.EventDomain);
                var pair = (libPath, target.EventId);
                if (!seenPairs.Add(pair)) continue;

                resultsByTarget.TryGetValue(pair, out var groupResults);
                groupResults ??= new List<SerializedReferenceUsageFinder.Usage>();

                string groupLabel = EventReferencePropertyDrawer.GetFullLabel(target.EventDomain, target.EventId);

                tabGroups.Add(new SearchTabGroup {
                    DomainPath = libPath,
                    Id = target.EventId,
                    Label = groupLabel,
                    ResultStartIndex = orderedResults.Count,
                    ResultCount = groupResults.Count,
                });

                orderedResults.AddRange(groupResults);
                domainIds.Add((target.EventDomain, target.EventId));
            }

            // Clear old results, keep targets state intact
            foreach (var state in activeRt.GroupHeaderStates) {
                if (state != null) DestroyImmediate(state);
            }
            activeRt.GroupHeaderStates.Clear();
            activeRt.GroupHeaderSerializedObjects.Clear();
            activeRt.GroupHeaderProperties.Clear();
            activeRt.GroupDomainProperties.Clear();
            activeRt.Results.Clear();
            activeRt.ResultObjectCache.Clear();
            activeRt.Scroll = Vector2.zero;

            activeTab.Groups.Clear();
            activeTab.Results.Clear();

            activeTab.Groups.AddRange(tabGroups);
            foreach (var usage in orderedResults) {
                activeTab.Results.Add(new SavedUsage {
                    Kind = usage.Kind, AssetPath = usage.AssetPath,
                    ObjectPath = usage.ObjectPath, OwnerType = usage.OwnerType,
                    PropertyPath = usage.PropertyPath,
                });
                activeRt.Results.Add(usage);
            }
            foreach (var (domain, id) in domainIds) {
                AddGroupHeaderToRuntime(activeRt, domain, id);
            }

            activeTab.Label = BuildTabLabel(targets);

            SaveSearchState();
            Repaint();
        }

        private static TabRuntime CreateTabRuntime() {
            var rt = new TabRuntime();
            var targetsState = CreateInstance<TabTargetsState>();
            targetsState.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            var so = new SerializedObject(targetsState);
            rt.TargetsState = targetsState;
            rt.TargetsSerializedObject = so;
            rt.TargetsProperty = so.FindProperty(nameof(TabTargetsState.targets));
            return rt;
        }

        private void SyncTabTargets(int tabIndex) {
            if (tabIndex < 0 || tabIndex >= _tabs.Count || tabIndex >= _tabRuntimes.Count) return;
            var tab = _tabs[tabIndex];
            var rt = _tabRuntimes[tabIndex];
            tab.TargetDomainPaths.Clear();
            tab.TargetIds.Clear();
            foreach (var t in rt.TargetsState.targets) {
                if (t.EventDomain == null) continue;
                tab.TargetDomainPaths.Add(AssetDatabase.GetAssetPath(t.EventDomain));
                tab.TargetIds.Add(t.EventId);
            }
        }

        private void ApplyRequestedSearchToActiveTab() {
            if (_requestedSearch == null || _requestedSearch.Length == 0) return;

            var rt = CreateTabRuntime();
            rt.TargetsState.targets.AddRange(_requestedSearch);
            rt.TargetsSerializedObject.Update();
            rt.TargetsProperty.isExpanded = true;

            _tabs.Add(new SearchTab { Label = "New" });
            _tabRuntimes.Add(rt);
            _activeTabIndex = _tabs.Count - 1;

            SyncTabTargets(_activeTabIndex);
            _tabs[_activeTabIndex].Label = BuildTabLabel(rt.TargetsState.targets);

            _requestedSearch = null;
            Repaint();
        }

        private void AddGroupHeaderToRuntime(TabRuntime runtime, EventDomain lib, int id) {
            var state = CreateInstance<SingleEventReferenceState>();
            state.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            if (lib != null) state.eventReference = new EventReference(lib, id);
            var so = new SerializedObject(state);
            var labelProp = so.FindProperty(nameof(SingleEventReferenceState.eventReference));
            runtime.GroupHeaderStates.Add(state);
            runtime.GroupHeaderSerializedObjects.Add(so);
            runtime.GroupHeaderProperties.Add(labelProp);
            runtime.GroupDomainProperties.Add(labelProp.FindPropertyRelative(EventReferencePropertyDrawer.EventDomainPropertyPath));
        }

        private void CloseTab(int index) {
            if (index < _tabRuntimes.Count) {
                var runtime = _tabRuntimes[index];
                foreach (var state in runtime.GroupHeaderStates) {
                    if (state != null) DestroyImmediate(state);
                }
                if (runtime.TargetsState != null) DestroyImmediate(runtime.TargetsState);
                _tabRuntimes.RemoveAt(index);
            }
            if (index < _tabs.Count) _tabs.RemoveAt(index);

            if (_activeTabIndex >= _tabs.Count) _activeTabIndex = _tabs.Count - 1;
            else if (_activeTabIndex > index) _activeTabIndex--;

            SaveSearchState();
        }

        private string BuildTabLabel(List<EventReference> targets) {
            int validCount = 0;
            string first = null;
            foreach (var t in targets) {
                if (t.EventDomain == null) continue;
                if (first == null) {
                    first = EventReferencePropertyDrawer.GetFullLabel(t.EventDomain, t.EventId);
                }
                validCount++;
            }
            if (first == null) return "New";
            return validCount == 1 ? first : $"{first} [+{validCount - 1}]";
        }

        private static void AddResultsUnique(
            IReadOnlyList<SerializedReferenceUsageFinder.Usage> usages,
            List<SerializedReferenceUsageFinder.Usage> results,
            HashSet<string> seen) {

            for (int i = 0; i < usages.Count; i++) {
                var usage = usages[i];

                string key =
                    $"{usage.Kind}|{usage.AssetPath}|{usage.ObjectPath}|{usage.OwnerType}|{usage.PropertyPath}";

                if (seen.Add(key))
                    results.Add(usage);
            }
        }

        private string[] GetValidSearchFolderPaths() {
            var paths = new List<string>();

            foreach (var folder in _state.searchFolders) {
                string path = GetFolderPath(folder);

                if (!string.IsNullOrEmpty(path))
                    paths.Add(path);
            }

            return paths.ToArray();
        }

        private static string GetFolderPath(DefaultAsset folder) {
            if (folder == null)
                return null;

            string path = AssetDatabase.GetAssetPath(folder);

            if (string.IsNullOrEmpty(path))
                return null;

            if (!AssetDatabase.IsValidFolder(path))
                return null;

            return path;
        }

        private static bool IsEventReference(
            SerializedReferenceUsageFinder.UsageCandidate<EventDomain> usage,
            EventDomain lib,
            HashSet<int> ids,
            out int matchedId) {
            matchedId = 0;
            var fieldType = usage.GetParentProperty()?.GetFieldInfo()?.FieldType;
            if (fieldType == null) return false;
            if (fieldType != typeof(EventReference)) return false;
            if (usage.Target != lib) return false;
            var idProp = usage.GetSiblingProperty(EventReferencePropertyDrawer.EventIdPropertyPath);
            if (idProp?.propertyType != SerializedPropertyType.Integer) return false;
            matchedId = idProp.intValue;
            return ids.Contains(matchedId);
        }

        private void SaveSearchState() {
            var saved = new SavedSearchState {
                SearchInAssets = _state.searchInAssets,
                SearchInOpenedScenes = _state.searchInOpenedScenes,
                SearchInAllScenes = _state.searchInAllScenes,
            };

            EditorPrefs.SetString(SearchStatePrefsKey, JsonUtility.ToJson(saved));
        }

        private void LoadSearchState() {
            string json = EditorPrefs.GetString(SearchStatePrefsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return;

            SavedSearchState saved;
            try { saved = JsonUtility.FromJson<SavedSearchState>(json); }
            catch { return; }
            if (saved == null) return;

            _state.searchInAssets = saved.SearchInAssets;
            _state.searchInOpenedScenes = saved.SearchInOpenedScenes;
            _state.searchInAllScenes = saved.SearchInAllScenes;
        }

        private void SaveSearchFolders()
        {
            var saved = new SavedFolders();

            foreach (DefaultAsset folder in _state.searchFolders)
            {
                string path = GetFolderPath(folder);

                if (!string.IsNullOrEmpty(path) && !saved.Paths.Contains(path))
                    saved.Paths.Add(path);
            }

            string json = JsonUtility.ToJson(saved);
            EditorPrefs.SetString(SearchFoldersPrefsKey, json);
        }

        private void LoadSearchFolders()
        {
            _state.searchFolders.Clear();

            string json = EditorPrefs.GetString(SearchFoldersPrefsKey, string.Empty);

            if (string.IsNullOrEmpty(json))
                return;

            SavedFolders saved;

            try
            {
                saved = JsonUtility.FromJson<SavedFolders>(json);
            }
            catch
            {
                saved = null;
            }

            if (saved?.Paths == null)
                return;

            foreach (string path in saved.Paths)
            {
                if (string.IsNullOrEmpty(path))
                    continue;

                if (!AssetDatabase.IsValidFolder(path))
                    continue;

                DefaultAsset folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);

                if (folder != null && !_state.searchFolders.Contains(folder))
                    _state.searchFolders.Add(folder);
            }
        }
    }

}
