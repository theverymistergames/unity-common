using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Editor.Search {

    public sealed class ObjectSearchWindow : EditorWindow {

        [Serializable]
        private sealed class SavedResult {
            public string kind;
            public string assetPath;
            public string objectPath;
            public string typeName;
        }

        [Serializable]
        private sealed class SearchTab {
            [SerializeReference] [SubclassSelector(includeEditor: true)] public IObjectSearchFilter filter;
            public List<SavedResult> results = new();
        }

        private sealed class PendingSearch {
            public AssetFileScanner scanner;
            public SearchTab tab;
            public IObjectSearchFilter filter;
        }

        [SerializeField] private List<SearchTab> _tabs = new();
        [SerializeField] private int _activeTabIndex = -1;
        [SerializeField] private bool _searchInPrefabs = true;
        [SerializeField] private bool _searchInOpenedScenes = true;
        [SerializeField] private bool _searchInAllScenes;
        [SerializeField] private List<DefaultAsset> _searchFolders = new();

        private SerializedObject _serializedObject;
        private PendingSearch _pendingSearch;
        private Vector2 _scroll;
        private GUIStyle _tabStyle;
        private SavedResult _selectedResult;
        private readonly Dictionary<string, Object> _objectCache = new();

        private const float RowHeight = 18f;
        private const float TypeColWidth = 160f;
        private const float TabMinWidth = 100f;
        private const float TabMaxWidth = 300f;

        [MenuItem("MisterGames/Tools/Object Search")]
        public static void Open() {
            var window = GetWindow<ObjectSearchWindow>("Object Search");
            window.minSize = new Vector2(520, 420);
        }

        private void OnEnable() {
            _serializedObject = new SerializedObject(this);

            if (_searchFolders.Count == 0) {
                var assetsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets");
                if (assetsFolder != null) _searchFolders.Add(assetsFolder);
            }

            if (_tabs.Count == 0) AddTab();
            if (_activeTabIndex < 0 || _activeTabIndex >= _tabs.Count) _activeTabIndex = 0;

            EditorApplication.update += OnEditorUpdate;
            SceneManager.sceneLoaded += OnSceneChanged;
            SceneManager.sceneUnloaded += OnSceneChanged;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneOpened;
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosed += OnSceneChanged;
        }

        private void OnDisable() {
            EditorApplication.update -= OnEditorUpdate;
            SceneManager.sceneLoaded -= OnSceneChanged;
            SceneManager.sceneUnloaded -= OnSceneChanged;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= OnSceneOpened;
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosed -= OnSceneChanged;

            CancelSearch();
        }

        private void OnSceneChanged(Scene scene, LoadSceneMode mode) => _objectCache.Clear();
        private void OnSceneChanged(Scene scene) => _objectCache.Clear();
        private void OnSceneOpened(Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode) => _objectCache.Clear();

        private void OnEditorUpdate() {
            if (_pendingSearch == null) return;

            if (_pendingSearch.scanner.IsDone) FinishSearch();
            Repaint();
        }

        private void OnGUI() {
            _serializedObject.Update();

            DrawSearchOptions();
            EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_searchFolders)), includeChildren: true);
            EditorGUILayout.Space(4);

            DrawTabBar();

            if (_activeTabIndex >= 0 && _activeTabIndex < _tabs.Count) {
                var filterProperty = _serializedObject
                    .FindProperty(nameof(_tabs))
                    .GetArrayElementAtIndex(_activeTabIndex)
                    .FindPropertyRelative(nameof(SearchTab.filter));

                EditorGUILayout.PropertyField(filterProperty, new GUIContent("Filter"), includeChildren: true);
            }

            _serializedObject.ApplyModifiedProperties();

            if (_activeTabIndex < 0 || _activeTabIndex >= _tabs.Count) return;

            var tab = _tabs[_activeTabIndex];

            EditorGUILayout.Space(4);
            DrawSearchButton(tab);
            EditorGUILayout.Space(4);
            DrawResults(tab);
        }

        private void DrawSearchOptions() {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search In", EditorStyles.boldLabel, GUILayout.Width(64));
            _searchInPrefabs = EditorGUILayout.ToggleLeft("Prefabs", _searchInPrefabs, GUILayout.Width(70));
            GUILayout.Space(8);
            _searchInOpenedScenes = EditorGUILayout.ToggleLeft("Opened Scenes", _searchInOpenedScenes, GUILayout.Width(108));
            GUILayout.Space(8);
            _searchInAllScenes = EditorGUILayout.ToggleLeft("All Scenes", _searchInAllScenes, GUILayout.Width(88));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabBar() {
            if (_tabStyle == null) {
                _tabStyle = new GUIStyle(EditorStyles.toolbarButton);
                _tabStyle.padding.left = 8;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            for (int i = 0; i < _tabs.Count; i++) {
                bool isActive = i == _activeTabIndex;
                var content = new GUIContent(GetTabLabel(_tabs[i]));
                float width = Mathf.Clamp(_tabStyle.CalcSize(content).x, TabMinWidth, TabMaxWidth);

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
                AddTab();
                GUIUtility.ExitGUI();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSearchButton(SearchTab tab) {
            if (_pendingSearch != null) {
                var scanner = _pendingSearch.scanner;
                if (GUILayout.Button($"Searching {scanner.ProcessedCount}/{scanner.TotalCount}... Cancel", GUILayout.Height(28))) CancelSearch();
                return;
            }

            using (new EditorGUI.DisabledScope(tab.filter == null)) {
                if (GUILayout.Button("Search", GUILayout.Height(28))) Search(tab);
            }
        }

        private void DrawResults(SearchTab tab) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Results: {tab.results.Count}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(tab.results.Count == 0)) {
                if (GUILayout.Button("Select Loaded", GUILayout.Width(100))) SelectLoaded(tab);
                if (GUILayout.Button("Clear", GUILayout.Width(60))) tab.results.Clear();
            }

            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < tab.results.Count; i++) {
                var rect = EditorGUILayout.GetControlRect(false, RowHeight);

                if (Event.current.type == EventType.Layout ||
                    rect.yMax < _scroll.y || rect.y > _scroll.y + position.height)
                {
                    continue;
                }

                DrawResultRow(rect, tab.results[i], i);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawResultRow(Rect rect, SavedResult result, int index) {
            if (index % 2 == 0) EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.08f));

            float typeWidth = Mathf.Min(TypeColWidth, rect.width * 0.4f);
            var objectRect = new Rect(rect.x, rect.y + 1f, rect.width - typeWidth - 4f, EditorGUIUtility.singleLineHeight);
            var typeRect = new Rect(objectRect.xMax + 4f, rect.y + 1f, typeWidth, EditorGUIUtility.singleLineHeight);

            var asset = AssetDatabase.LoadMainAssetAtPath(result.assetPath);
            var target = ResolveObject(result);
            string label = $"{System.IO.Path.GetFileNameWithoutExtension(result.assetPath)}: {result.objectPath}";
            string tooltip = $"{result.kind}\n{result.assetPath}\n{result.objectPath}";

            var icon = target != null ? AssetPreview.GetMiniThumbnail(target) : asset != null ? AssetPreview.GetMiniThumbnail(asset) : null;

            if (GUI.Button(objectRect, new GUIContent(label, icon, tooltip), EditorStyles.objectField)) {
                _selectedResult = result;

                var ping = target != null ? target : asset;

                if (ping != null) {
                    Selection.activeObject = ping;
                    EditorGUIUtility.PingObject(ping);

                    if (target == null && Event.current.clickCount >= 2) AssetDatabase.OpenAsset(asset);
                }
            }

            EditorGUI.LabelField(typeRect, result.typeName, EditorStyles.miniLabel);

            if (_selectedResult == result) SearchResultHighlight.Draw(rect);
        }

        private void SelectLoaded(SearchTab tab) {
            var objects = new List<Object>();

            for (int i = 0; i < tab.results.Count; i++) {
                var obj = ResolveObject(tab.results[i]);
                if (obj != null) objects.Add(obj);
            }

            Selection.objects = objects.ToArray();
        }

        private void Search(SearchTab tab) {
            if (_pendingSearch != null || tab.filter == null) return;

            var folders = new List<string>();

            for (int i = 0; i < _searchFolders.Count; i++) {
                string path = _searchFolders[i] != null ? AssetDatabase.GetAssetPath(_searchFolders[i]) : null;
                if (!string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path)) folders.Add(path);
            }

            bool searchOpenedScenes = _searchInOpenedScenes && !_searchInAllScenes;

            if (folders.Count == 0 && !searchOpenedScenes) return;
            if ((searchOpenedScenes || _searchInAllScenes) && !AssetFileScanner.SaveOpenedScenesIfUserWantsTo()) return;

            var request = new AssetFileScanner.Request();
            request.AddFiles(folders.ToArray(), assets: false, prefabs: _searchInPrefabs, scenes: _searchInAllScenes);
            if (searchOpenedScenes) request.AddOpenedScenes();

            tab.filter.SetupRequest(request);

            _pendingSearch = new PendingSearch {
                scanner = AssetFileScanner.Start(request),
                tab = tab,
                filter = tab.filter,
            };
        }

        private void FinishSearch() {
            var pendingSearch = _pendingSearch;
            _pendingSearch = null;

            pendingSearch.scanner.Complete();
            if (pendingSearch.scanner.IsCancelled || !_tabs.Contains(pendingSearch.tab)) return;

            var results = pendingSearch.tab.results;
            var files = pendingSearch.scanner.Files;

            results.Clear();
            _objectCache.Clear();
            _scroll = Vector2.zero;

            for (int f = 0; f < files.Count; f++) {
                var file = files[f];

                for (int o = 0; o < file.objects.Count; o++) {
                    var obj = file.objects[o];
                    if (!pendingSearch.filter.IsMatch(file, obj)) continue;

                    results.Add(new SavedResult {
                        kind = file.kind,
                        assetPath = file.assetPath,
                        objectPath = obj.path,
                        typeName = GetTypeName(obj),
                    });
                }
            }

            Repaint();
        }

        private void CancelSearch() {
            _pendingSearch?.scanner.Cancel();
            _pendingSearch = null;
        }

        private void AddTab() {
            _tabs.Add(new SearchTab());
            _activeTabIndex = _tabs.Count - 1;
            _scroll = Vector2.zero;
        }

        private void CloseTab(int index) {
            if (_pendingSearch != null && _pendingSearch.tab == _tabs[index]) CancelSearch();

            _tabs.RemoveAt(index);

            if (_tabs.Count == 0) AddTab();
            else if (_activeTabIndex >= _tabs.Count) _activeTabIndex = _tabs.Count - 1;
            else if (_activeTabIndex > index) _activeTabIndex--;
        }

        private static string GetTabLabel(SearchTab tab) {
            string name = tab.filter == null ? "New" : ObjectNames.NicifyVariableName(tab.filter.GetType().Name);
            return tab.results.Count > 0 ? $"{name} ({tab.results.Count})" : name;
        }

        private static string GetTypeName(AssetFileScanner.ScannedObject obj) {
            if (!string.IsNullOrEmpty(obj.scriptGuid)) {
                var type = AssetFileScanner.GetScriptType(obj.scriptGuid);
                if (type != null) return type.Name;
            }

            return ObjectSearchClassIds.GetName(obj.classId);
        }

        private Object ResolveObject(SavedResult result) {
            string key = $"{result.assetPath}|{result.objectPath}";
            if (_objectCache.TryGetValue(key, out var cached) && cached != null) return cached;

            GameObject root = null;

            if (result.kind == AssetFileScanner.KindScene) {
                var scene = SceneManager.GetSceneByPath(result.assetPath);
                if (scene.IsValid() && scene.isLoaded) root = FindRoot(scene.GetRootGameObjects(), result.objectPath);
            }
            else if (result.kind == AssetFileScanner.KindPrefab) {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(result.assetPath);
                if (prefab != null) root = prefab;
            }

            var obj = root != null ? FindByPath(root, result.objectPath) : null;
            _objectCache[key] = obj;

            return obj;
        }

        private static GameObject FindRoot(GameObject[] roots, string objectPath) {
            string rootName = GetRootName(objectPath);

            for (int i = 0; i < roots.Length; i++) {
                if (roots[i].name == rootName) return roots[i];
            }

            return null;
        }

        private static GameObject FindByPath(GameObject root, string objectPath) {
            int sep = objectPath.IndexOf('/');
            if (sep < 0) return root;

            var child = root.transform.Find(objectPath[(sep + 1)..]);
            return child != null ? child.gameObject : null;
        }

        private static string GetRootName(string objectPath) {
            int sep = objectPath.IndexOf('/');
            return sep < 0 ? objectPath : objectPath[..sep];
        }
    }

}
