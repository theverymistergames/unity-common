using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MisterGames.Scenario.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MisterGames.Scenario.Editor.Events {

    [InitializeOnLoad]
    public static class EventDomainUsagesCache {

        public static event Action Changed;

        public static int Version { get; private set; }
        public static bool IsSearching => _inFlightCount > 0;

        private const string EventDomainPropertyPath = "_eventDomain";
        private const string PrefabInstanceLine = "PrefabInstance:";
        private const string SceneExtension = ".unity";
        private const string PrefabExtension = ".prefab";
        private const string AssetsFolder = "Assets/";
        private const double LiveUpdateInterval = 0.2;
        private const double SearchNotifyInterval = 1.0;
        private const int MaxPrefabDepth = 64;

        private const int ClassIdGameObject = 1;
        private const int ClassIdTransform = 4;
        private const int ClassIdMonoBehaviour = 114;
        private const int ClassIdRectTransform = 224;
        private const int ClassIdPrefabInstance = 1001;
        private const int ClassIdSceneRoots = 1660057539;

        public sealed class UsageGroup {

            public readonly string assetPath;
            public readonly List<Usage> usages = new();

            public UsageGroup(string assetPath) {
                this.assetPath = assetPath;
            }
        }

        public sealed class Usage {

            public readonly string domainGuid;
            public readonly GameObject liveObject;
            internal readonly Segment[] segments;
            internal readonly long localId;
            private readonly string _prefabGuid;

            public string Path => _path ??= BuildDisplayPath();
            public string PrefabPath => _prefabPath ??= string.IsNullOrEmpty(_prefabGuid) ? null : AssetDatabase.GUIDToAssetPath(_prefabGuid);
            public bool IsPrefabInstance => !string.IsNullOrEmpty(PrefabPath);
            public bool IsLive => segments == null;

            private string _path;
            private string _prefabPath;

            internal Usage(string domainGuid, Segment[] segments, long localId, string prefabGuid = null) {
                this.domainGuid = domainGuid;
                this.segments = segments;
                this.localId = localId;
                _prefabGuid = prefabGuid;
            }

            internal Usage(string domainGuid, GameObject liveObject, string path, string prefabPath) {
                this.domainGuid = domainGuid;
                this.liveObject = liveObject;
                _path = path;
                _prefabPath = prefabPath;
            }

            private string BuildDisplayPath() {
                var names = new string[segments.Length];
                for (int i = 0; i < segments.Length; i++) {
                    names[i] = segments[i].GetDisplayName();
                }
                return string.Join("/", names);
            }
        }

        internal readonly struct Segment {

            public readonly string name;
            public readonly string sourcePrefabGuid;
            public readonly int siblingIndex;

            public Segment(string name, string sourcePrefabGuid, int siblingIndex) {
                this.name = name;
                this.sourcePrefabGuid = sourcePrefabGuid;
                this.siblingIndex = siblingIndex;
            }

            public string GetDisplayName() {
                if (!string.IsNullOrEmpty(name)) return name;
                if (string.IsNullOrEmpty(sourcePrefabGuid)) return "?";

                string path = AssetDatabase.GUIDToAssetPath(sourcePrefabGuid);
                return string.IsNullOrEmpty(path) ? "?" : System.IO.Path.GetFileNameWithoutExtension(path);
            }
        }

        private readonly struct DomainOverride {

            public readonly long targetId;
            public readonly string targetGuid;
            public readonly string domainGuid;

            public DomainOverride(long targetId, string targetGuid, string domainGuid) {
                this.targetId = targetId;
                this.targetGuid = targetGuid;
                this.domainGuid = domainGuid;
            }
        }

        private sealed class InstanceData {

            public readonly long id;
            public readonly string sourcePrefabGuid;
            public readonly Segment[] segments;
            public readonly List<DomainOverride> overrides;

            public InstanceData(long id, string sourcePrefabGuid, Segment[] segments, List<DomainOverride> overrides) {
                this.id = id;
                this.sourcePrefabGuid = sourcePrefabGuid;
                this.segments = segments;
                this.overrides = overrides;
            }
        }

        private sealed class FileData {
            public readonly List<Usage> runners = new();
            public readonly List<InstanceData> instances = new();
        }

        private readonly struct Job {

            public readonly string assetPath;
            public readonly string fullPath;
            public readonly string guid;
            public readonly int version;

            public Job(string assetPath, string fullPath, string guid, int version) {
                this.assetPath = assetPath;
                this.fullPath = fullPath;
                this.guid = guid;
                this.version = version;
            }
        }

        private readonly struct Result {

            public readonly Job job;
            public readonly FileData data;

            public Result(Job job, FileData data) {
                this.job = job;
                this.data = data;
            }
        }

        private sealed class Doc {
            public int classId;
            public long id;
            public bool stripped;
            public string name;
            public long gameObject;
            public long father;
            public long prefabInstance;
            public List<long> children;
            public string scriptGuid;
            public string domainGuid;
            public long transformParent;
            public string sourcePrefabGuid;
            public string instanceName;
            public List<DomainOverride> domainOverrides;
        }

        private sealed class FileContext {
            public readonly Dictionary<long, Doc> docs = new();
            public readonly Dictionary<long, long> transformByGameObject = new();
            public List<long> roots;
            public bool isPrefab;
        }

        private static readonly List<Usage> EmptyUsages = new();

        private static readonly Dictionary<string, FileData> _fileData = new();
        private static readonly Dictionary<string, string> _fileGuids = new();
        private static readonly Dictionary<string, string> _pathByGuid = new();
        private static readonly Dictionary<string, int> _fileVersions = new();
        private static readonly Dictionary<string, List<Usage>> _resolvedUsages = new();
        private static readonly HashSet<string> _resolveStack = new();
        private static Dictionary<string, List<Usage>> _liveUsages = new();
        private static Dictionary<string, List<Usage>> _liveUsagesBuffer = new();
        private static readonly ConcurrentQueue<Job> _jobs = new();
        private static readonly ConcurrentQueue<Result> _results = new();
        private static readonly List<EventDomainRunner> _runnersBuffer = new();

        private static CancellationTokenSource _cts = new();
        private static Task _worker;
        private static string _runnerScriptGuid;
        private static string _projectFolder;
        private static int _inFlightCount;
        private static bool _isStarted;
        private static bool _isResolvedDirty = true;
        private static bool _isLiveDirty = true;
        private static double _lastLiveUpdateTime = double.MinValue;
        private static double _lastFileNotifyTime = double.MinValue;
        private static bool _hasPendingFileChanges;

        static EventDomainUsagesCache() {
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnBeforeAssemblyReload;

            ObjectChangeEvents.changesPublished += OnObjectChangesPublished;
            Undo.undoRedoPerformed += SetLiveDirty;
            EditorSceneManager.sceneOpened += (_, _) => SetLiveDirty();
            EditorSceneManager.sceneClosed += _ => SetLiveDirty();
            EditorSceneManager.newSceneCreated += (_, _, _) => SetLiveDirty();
            EditorSceneManager.sceneSaved += _ => SetLiveDirty();
            PrefabStage.prefabStageOpened += _ => SetLiveDirty();
            PrefabStage.prefabStageClosing += _ => SetLiveDirty();
            EditorApplication.playModeStateChanged += _ => SetLiveDirty();
        }

        public static void GetUsages(EventDomain eventDomain, List<UsageGroup> result) {
            result.Clear();
            if (eventDomain == null) return;

            EnsureStarted();
            if (_isLiveDirty && CanUpdateLive() && UpdateLiveUsages()) NotifyChanged();

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(eventDomain, out string domainGuid, out long _)) return;

            if (_isResolvedDirty) {
                _isResolvedDirty = false;
                _resolvedUsages.Clear();
            }

            foreach (string assetPath in _fileData.Keys) {
                if (_liveUsages.ContainsKey(assetPath)) continue;
                AddGroup(assetPath, ResolveUsages(assetPath, depth: 0), domainGuid, result);
            }

            foreach ((string assetPath, var usages) in _liveUsages) {
                AddGroup(assetPath, usages, domainGuid, result);
            }

            result.Sort((a, b) => string.CompareOrdinal(a.assetPath, b.assetPath));
        }

        public static void PingAsset(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) return;

            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset != null) EditorGUIUtility.PingObject(asset);
        }

        public static void SelectUsage(EventDomain eventDomain, UsageGroup group, Usage usage) {
            if (usage.IsLive) {
                if (usage.liveObject == null) {
                    SetLiveDirty();
                    return;
                }

                SelectObject(usage.liveObject);
                return;
            }

            IReadOnlyList<Transform> roots;

            if (group.assetPath.EndsWith(SceneExtension, StringComparison.OrdinalIgnoreCase)) {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

                var scene = EditorSceneManager.OpenScene(group.assetPath, OpenSceneMode.Single);
                if (!scene.IsValid()) return;

                var rootObjects = scene.GetRootGameObjects();
                var transforms = new Transform[rootObjects.Length];
                for (int i = 0; i < rootObjects.Length; i++) {
                    transforms[i] = rootObjects[i].transform;
                }

                roots = transforms;
            }
            else {
                var stage = PrefabStageUtility.OpenPrefab(group.assetPath);
                if (stage == null) return;

                roots = new[] { stage.prefabContentsRoot.transform };
            }

            var target = ResolveTransform(roots, usage.segments);
            var runner = FindRunner(target != null ? new[] { target } : roots, eventDomain);

            if (runner != null) SelectObject(runner.gameObject);
            else if (target != null) SelectObject(target.gameObject);
        }

        private static List<Usage> ResolveUsages(string assetPath, int depth) {
            if (_resolvedUsages.TryGetValue(assetPath, out var usages)) return usages;
            if (!_fileData.TryGetValue(assetPath, out var data)) return EmptyUsages;
            if (depth > MaxPrefabDepth || !_resolveStack.Add(assetPath)) return EmptyUsages;

            usages = data.runners;

            for (int i = 0; i < data.instances.Count; i++) {
                var instance = data.instances[i];

                var sourceUsages = instance.sourcePrefabGuid != null &&
                                   _pathByGuid.TryGetValue(instance.sourcePrefabGuid, out string sourcePath)
                    ? ResolveUsages(sourcePath, depth + 1)
                    : EmptyUsages;

                if (sourceUsages.Count == 0 && instance.overrides == null) continue;
                if (ReferenceEquals(usages, data.runners)) usages = new List<Usage>(data.runners);

                AddInstanceUsages(instance, sourceUsages, usages);
            }

            _resolveStack.Remove(assetPath);
            _resolvedUsages[assetPath] = usages;

            return usages;
        }

        private static void AddInstanceUsages(InstanceData instance, List<Usage> sourceUsages, List<Usage> usages) {
            var overrides = instance.overrides;
            int matchedOverrides = 0;

            for (int i = 0; i < sourceUsages.Count; i++) {
                var sourceUsage = sourceUsages[i];
                string domainGuid = sourceUsage.domainGuid;

                if (overrides != null) {
                    for (int j = 0; j < overrides.Count; j++) {
                        var domainOverride = overrides[j];
                        if (domainOverride.targetId != sourceUsage.localId ||
                            domainOverride.targetGuid != instance.sourcePrefabGuid
                        ) {
                            continue;
                        }

                        domainGuid = domainOverride.domainGuid;
                        matchedOverrides++;
                        break;
                    }
                }

                if (domainGuid == null) continue;

                usages.Add(new Usage(
                    domainGuid,
                    ConcatSegments(instance.segments, sourceUsage.segments),
                    (sourceUsage.localId ^ instance.id) & long.MaxValue,
                    instance.sourcePrefabGuid
                ));
            }

            if (overrides == null || matchedOverrides >= overrides.Count) return;

            for (int i = 0; i < overrides.Count; i++) {
                var domainOverride = overrides[i];
                if (domainOverride.domainGuid == null || IsMatched(domainOverride, sourceUsages, instance)) continue;

                usages.Add(new Usage(domainOverride.domainGuid, instance.segments, instance.id, instance.sourcePrefabGuid));
            }
        }

        private static bool IsMatched(DomainOverride domainOverride, List<Usage> sourceUsages, InstanceData instance) {
            if (domainOverride.targetGuid != instance.sourcePrefabGuid) return false;

            for (int i = 0; i < sourceUsages.Count; i++) {
                if (sourceUsages[i].localId == domainOverride.targetId) return true;
            }

            return false;
        }

        private static Segment[] ConcatSegments(Segment[] instanceSegments, Segment[] sourceSegments) {
            int sourceCount = Math.Max(0, sourceSegments.Length - 1);
            var segments = new Segment[instanceSegments.Length + sourceCount];

            Array.Copy(instanceSegments, segments, instanceSegments.Length);
            if (sourceCount > 0) Array.Copy(sourceSegments, 1, segments, instanceSegments.Length, sourceCount);

            return segments;
        }

        private static void AddGroup(string assetPath, List<Usage> usages, string domainGuid, List<UsageGroup> result) {
            UsageGroup group = null;

            for (int i = 0; i < usages.Count; i++) {
                var usage = usages[i];
                if (usage.domainGuid != domainGuid) continue;

                group ??= new UsageGroup(assetPath);
                group.usages.Add(usage);
            }

            if (group != null) result.Add(group);
        }

        private static void SelectObject(GameObject gameObject) {
            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }

        private static Transform ResolveTransform(IReadOnlyList<Transform> roots, Segment[] segments) {
            Transform current = null;

            for (int i = 0; i < segments.Length; i++) {
                var segment = segments[i];
                int count = current == null ? roots.Count : current.childCount;
                string name = segment.GetDisplayName();

                var byIndex = segment.siblingIndex >= 0 && segment.siblingIndex < count
                    ? GetChild(roots, current, segment.siblingIndex)
                    : null;

                if (byIndex != null && byIndex.name == name) {
                    current = byIndex;
                    continue;
                }

                Transform byName = null;
                for (int j = 0; j < count; j++) {
                    var child = GetChild(roots, current, j);
                    if (child.name != name) continue;

                    byName = child;
                    break;
                }

                var next = byName != null ? byName : byIndex;
                if (next == null) break;

                current = next;
            }

            return current;
        }

        private static Transform GetChild(IReadOnlyList<Transform> roots, Transform parent, int index) {
            return parent == null ? roots[index] : parent.GetChild(index);
        }

        private static EventDomainRunner FindRunner(IReadOnlyList<Transform> roots, EventDomain eventDomain) {
            for (int i = 0; i < roots.Count; i++) {
                roots[i].GetComponentsInChildren(includeInactive: true, _runnersBuffer);

                for (int j = 0; j < _runnersBuffer.Count; j++) {
                    var runner = _runnersBuffer[j];
                    if (runner.GetEventDomain() != eventDomain) continue;

                    _runnersBuffer.Clear();
                    return runner;
                }
            }

            _runnersBuffer.Clear();
            return null;
        }

        private static void EnsureStarted() {
            if (_isStarted) return;

            _runnerScriptGuid = FindRunnerScriptGuid();
            if (string.IsNullOrEmpty(_runnerScriptGuid)) return;

            _isStarted = true;
            _projectFolder = Directory.GetParent(Application.dataPath).FullName;

            string[] paths = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < paths.Length; i++) {
                string path = paths[i];
                if (IsSearchTarget(path)) Enqueue(path);
            }
        }

        private static string FindRunnerScriptGuid() {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(MonoScript)} {nameof(EventDomainRunner)}");

            for (int i = 0; i < guids.Length; i++) {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (script != null && script.GetClass() == typeof(EventDomainRunner)) return guids[i];
            }

            return null;
        }

        private static bool IsSearchTarget(string assetPath) {
            return assetPath.StartsWith(AssetsFolder, StringComparison.Ordinal) &&
                   (assetPath.EndsWith(SceneExtension, StringComparison.OrdinalIgnoreCase) ||
                    assetPath.EndsWith(PrefabExtension, StringComparison.OrdinalIgnoreCase));
        }

        private static void Enqueue(string assetPath) {
            int version = _fileVersions.GetValueOrDefault(assetPath) + 1;
            _fileVersions[assetPath] = version;
            _inFlightCount++;

            var job = new Job(assetPath, Path.Combine(_projectFolder, assetPath), AssetDatabase.AssetPathToGUID(assetPath), version);
            _jobs.Enqueue(job);

            EnsureWorker();
        }

        private static void Remove(string assetPath) {
            _fileVersions[assetPath] = _fileVersions.GetValueOrDefault(assetPath) + 1;
            if (RemoveFileData(assetPath)) NotifyChanged();
        }

        private static void SetFileData(string assetPath, string guid, FileData data) {
            RemoveFileData(assetPath);

            _fileData[assetPath] = data;
            _isResolvedDirty = true;

            if (string.IsNullOrEmpty(guid)) return;

            _fileGuids[assetPath] = guid;
            _pathByGuid[guid] = assetPath;
        }

        private static bool RemoveFileData(string assetPath) {
            if (_fileGuids.Remove(assetPath, out string guid) &&
                _pathByGuid.TryGetValue(guid, out string path) && path == assetPath
            ) {
                _pathByGuid.Remove(guid);
            }

            if (!_fileData.Remove(assetPath)) return false;

            _isResolvedDirty = true;
            return true;
        }

        private static void EnsureWorker() {
            if (_worker is { IsCompleted: false } || _jobs.IsEmpty) return;

            var token = _cts.Token;
            _worker = Task.Run(() => ProcessJobs(token), token);
        }

        private static void ProcessJobs(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested && _jobs.TryDequeue(out var job)) {
                FileData data;

                try {
                    data = ParseFile(job.assetPath, File.ReadAllText(job.fullPath), _runnerScriptGuid);
                }
                catch (Exception) {
                    data = null;
                }

                _results.Enqueue(new Result(job, data));
            }
        }

        private static void Update() {
            while (_results.TryDequeue(out var result)) {
                _inFlightCount--;

                var job = result.job;
                if (_fileVersions.GetValueOrDefault(job.assetPath) != job.version) continue;

                if (result.data != null) SetFileData(job.assetPath, job.guid, result.data);
                else RemoveFileData(job.assetPath);

                _hasPendingFileChanges = true;
            }

            if (_isStarted) EnsureWorker();

            bool changed = false;
            double time = EditorApplication.timeSinceStartup;

            if (_hasPendingFileChanges && (!IsSearching || time >= _lastFileNotifyTime + SearchNotifyInterval)) {
                _hasPendingFileChanges = false;
                _lastFileNotifyTime = time;
                changed = true;
            }

            if (_isStarted && _isLiveDirty && Changed != null && CanUpdateLive()) {
                changed |= UpdateLiveUsages();
            }

            if (changed) NotifyChanged();
        }

        private static void NotifyChanged() {
            Version++;
            Changed?.Invoke();
        }

        private static void OnBeforeAssemblyReload() {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        private static void OnObjectChangesPublished(ref ObjectChangeEventStream stream) {
            if (_isLiveDirty) return;

            for (int i = 0; i < stream.length; i++) {
                if (!IsLiveUsagesChange(ref stream, i)) continue;

                SetLiveDirty();
                return;
            }
        }

        private static bool IsLiveUsagesChange(ref ObjectChangeEventStream stream, int index) {
            switch (stream.GetEventType(index)) {
                case ObjectChangeKind.ChangeScene:
                case ObjectChangeKind.CreateGameObjectHierarchy:
                case ObjectChangeKind.DestroyGameObjectHierarchy:
                case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
                case ObjectChangeKind.ChangeGameObjectStructure:
                case ObjectChangeKind.ChangeGameObjectParent:
                case ObjectChangeKind.UpdatePrefabInstances:
                    return true;

                case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
                    stream.GetChangeGameObjectOrComponentPropertiesEvent(index, out var data);
                    return EditorUtility.EntityIdToObject(data.entityId) is EventDomainRunner or GameObject;

                default:
                    return false;
            }
        }

        private static void SetLiveDirty() {
            _isLiveDirty = true;
        }

        private static bool CanUpdateLive() {
            return EditorApplication.timeSinceStartup >= _lastLiveUpdateTime + LiveUpdateInterval;
        }

        private static bool UpdateLiveUsages() {
            _isLiveDirty = false;
            _lastLiveUpdateTime = EditorApplication.timeSinceStartup;
            _liveUsagesBuffer.Clear();

            for (int i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                var usages = GetOrCreateLiveList(_liveUsagesBuffer, scene.path);
                var rootObjects = scene.GetRootGameObjects();

                for (int j = 0; j < rootObjects.Length; j++) {
                    CollectLiveUsages(rootObjects[j].transform, usages);
                }
            }

            if (PrefabStageUtility.GetCurrentPrefabStage() is { } stage && stage.prefabContentsRoot != null) {
                var usages = GetOrCreateLiveList(_liveUsagesBuffer, stage.assetPath);
                usages.Clear();
                CollectLiveUsages(stage.prefabContentsRoot.transform, usages);
            }

            if (AreLiveUsagesEqual(_liveUsages, _liveUsagesBuffer)) return false;

            (_liveUsages, _liveUsagesBuffer) = (_liveUsagesBuffer, _liveUsages);
            return true;
        }

        private static bool AreLiveUsagesEqual(Dictionary<string, List<Usage>> a, Dictionary<string, List<Usage>> b) {
            if (a.Count != b.Count) return false;

            foreach ((string assetPath, var usagesA) in a) {
                if (!b.TryGetValue(assetPath, out var usagesB) || usagesA.Count != usagesB.Count) return false;

                for (int i = 0; i < usagesA.Count; i++) {
                    var usageA = usagesA[i];
                    var usageB = usagesB[i];

                    if (usageA.liveObject != usageB.liveObject ||
                        usageA.domainGuid != usageB.domainGuid ||
                        usageA.Path != usageB.Path ||
                        usageA.PrefabPath != usageB.PrefabPath
                    ) {
                        return false;
                    }
                }
            }

            return true;
        }

        private static List<Usage> GetOrCreateLiveList(Dictionary<string, List<Usage>> liveUsages, string assetPath) {
            if (liveUsages.TryGetValue(assetPath, out var usages)) return usages;

            usages = new List<Usage>();
            liveUsages[assetPath] = usages;
            return usages;
        }

        private static void CollectLiveUsages(Transform root, List<Usage> usages) {
            root.GetComponentsInChildren(includeInactive: true, _runnersBuffer);

            for (int i = 0; i < _runnersBuffer.Count; i++) {
                var runner = _runnersBuffer[i];
                var eventDomain = runner.GetEventDomain();

                if (eventDomain == null ||
                    !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(eventDomain, out string domainGuid, out long _)
                ) {
                    continue;
                }

                usages.Add(new Usage(domainGuid, runner.gameObject, GetTransformPath(runner.transform), GetLivePrefabPath(runner)));
            }

            _runnersBuffer.Clear();
        }

        private static string GetLivePrefabPath(EventDomainRunner runner) {
            if (!PrefabUtility.IsPartOfPrefabInstance(runner) || PrefabUtility.IsAddedComponentOverride(runner)) return null;

            var root = PrefabUtility.GetOutermostPrefabInstanceRoot(runner);
            return root != null ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root) : null;
        }

        private static string GetTransformPath(Transform transform) {
            var names = new List<string>();

            for (var t = transform; t != null; t = t.parent) {
                names.Add(t.name);
            }

            names.Reverse();
            return string.Join("/", names);
        }

        private static FileData ParseFile(string assetPath, string text, string runnerScriptGuid) {
            if (!text.Contains(runnerScriptGuid) && !text.Contains(PrefabInstanceLine)) return null;

            var ctx = new FileContext { isPrefab = assetPath.EndsWith(PrefabExtension, StringComparison.OrdinalIgnoreCase) };

            Doc doc = null;
            List<long> list = null;
            string modificationPath = null;
            long modificationTargetId = 0;
            string modificationTargetGuid = null;

            using var reader = new StringReader(text);

            while (reader.ReadLine() is { } line) {
                if (line.StartsWith("--- !u!", StringComparison.Ordinal)) {
                    doc = ParseHeader(line);
                    ctx.docs[doc.id] = doc;
                    list = null;
                    modificationPath = null;
                    continue;
                }

                if (doc == null) continue;

                if (list != null) {
                    if (line.StartsWith("  - {fileID: ", StringComparison.Ordinal)) {
                        list.Add(ParseFileId(line));
                        continue;
                    }

                    list = null;
                }

                switch (doc.classId) {
                    case ClassIdGameObject:
                        if (TryGetValue(line, "  m_Name: ", out string name)) doc.name = Unquote(name);
                        else if (TryGetValue(line, "  m_PrefabInstance: ", out string prefabInstance)) doc.prefabInstance = ParseFileId(prefabInstance);
                        break;

                    case ClassIdTransform:
                    case ClassIdRectTransform:
                        if (TryGetValue(line, "  m_GameObject: ", out string gameObject)) doc.gameObject = ParseFileId(gameObject);
                        else if (TryGetValue(line, "  m_Father: ", out string father)) doc.father = ParseFileId(father);
                        else if (TryGetValue(line, "  m_PrefabInstance: ", out string prefabInstance)) doc.prefabInstance = ParseFileId(prefabInstance);
                        else if (line.StartsWith("  m_Children:", StringComparison.Ordinal)) list = doc.children = new List<long>();
                        break;

                    case ClassIdMonoBehaviour:
                        if (TryGetValue(line, "  m_GameObject: ", out string owner)) doc.gameObject = ParseFileId(owner);
                        else if (TryGetValue(line, "  m_Script: ", out string script)) doc.scriptGuid = ParseGuid(script);
                        else if (TryGetValue(line, $"  {EventDomainPropertyPath}: ", out string domain)) doc.domainGuid = ParseGuid(domain);
                        break;

                    case ClassIdPrefabInstance:
                        if (TryGetValue(line, "    m_TransformParent: ", out string transformParent)) {
                            doc.transformParent = ParseFileId(transformParent);
                        }
                        else if (TryGetValue(line, "  m_SourcePrefab: ", out string sourcePrefab)) {
                            doc.sourcePrefabGuid = ParseGuid(sourcePrefab);
                        }
                        else if (TryGetValue(line, "    - target: ", out string target)) {
                            modificationPath = null;
                            modificationTargetId = ParseFileId(target);
                            modificationTargetGuid = ParseGuid(target);
                        }
                        else if (TryGetValue(line, "      propertyPath: ", out string propertyPath)) {
                            modificationPath = propertyPath.Trim();
                        }
                        else if (modificationPath == "m_Name" && doc.instanceName == null &&
                                 TryGetValue(line, "      value: ", out string value)
                        ) {
                            doc.instanceName = Unquote(value);
                        }
                        else if (modificationPath == EventDomainPropertyPath &&
                                 TryGetValue(line, "      objectReference: ", out string objectReference)
                        ) {
                            var domainOverride = new DomainOverride(modificationTargetId, modificationTargetGuid, ParseGuid(objectReference));
                            (doc.domainOverrides ??= new List<DomainOverride>()).Add(domainOverride);
                        }
                        break;

                    case ClassIdSceneRoots:
                        if (line.StartsWith("  m_Roots:", StringComparison.Ordinal)) list = ctx.roots = new List<long>();
                        break;
                }
            }

            foreach (var d in ctx.docs.Values) {
                if (d.stripped || d.gameObject == 0 || d.classId is not (ClassIdTransform or ClassIdRectTransform)) continue;
                ctx.transformByGameObject[d.gameObject] = d.id;
            }

            FileData data = null;

            foreach (var d in ctx.docs.Values) {
                if (d.classId == ClassIdMonoBehaviour && d.scriptGuid == runnerScriptGuid && d.domainGuid != null) {
                    (data ??= new FileData()).runners.Add(new Usage(d.domainGuid, BuildGameObjectPath(ctx, d.gameObject), d.id));
                    continue;
                }

                if (d.classId == ClassIdPrefabInstance && d.sourcePrefabGuid != null) {
                    var instance = new InstanceData(d.id, d.sourcePrefabGuid, BuildPrefabInstancePath(ctx, d.id), d.domainOverrides);
                    (data ??= new FileData()).instances.Add(instance);
                }
            }

            return data;
        }

        private static Segment[] BuildGameObjectPath(FileContext ctx, long gameObjectId) {
            var segments = new List<Segment>();
            if (!ctx.docs.TryGetValue(gameObjectId, out var gameObject)) return segments.ToArray();

            long transform = gameObject.stripped
                ? AddPrefabInstanceSegment(ctx, gameObject.prefabInstance, segments)
                : ctx.transformByGameObject.GetValueOrDefault(gameObjectId);

            AddTransformSegments(ctx, transform, segments);

            segments.Reverse();
            return segments.ToArray();
        }

        private static Segment[] BuildPrefabInstancePath(FileContext ctx, long prefabInstanceId) {
            var segments = new List<Segment>();

            long transform = AddPrefabInstanceSegment(ctx, prefabInstanceId, segments);
            AddTransformSegments(ctx, transform, segments);

            segments.Reverse();
            return segments.ToArray();
        }

        private static void AddTransformSegments(FileContext ctx, long transform, List<Segment> segments) {
            int guard = 0;

            while (transform != 0 && guard++ < 1000 && ctx.docs.TryGetValue(transform, out var doc)) {
                if (doc.stripped) {
                    transform = AddPrefabInstanceSegment(ctx, doc.prefabInstance, segments);
                    continue;
                }

                string name = ctx.docs.TryGetValue(doc.gameObject, out var gameObject) ? gameObject.name : null;
                segments.Add(new Segment(name, null, GetSiblingIndex(ctx, doc.father, transform)));

                transform = doc.father;
            }
        }

        private static long AddPrefabInstanceSegment(FileContext ctx, long prefabInstanceId, List<Segment> segments) {
            if (!ctx.docs.TryGetValue(prefabInstanceId, out var instance)) return 0;

            long parent = instance.transformParent;
            var siblings = parent == 0 ? ctx.roots : ctx.docs.GetValueOrDefault(parent)?.children;
            int siblingIndex = parent == 0 && ctx.isPrefab ? 0 : -1;

            if (siblings != null) {
                for (int i = 0; i < siblings.Count; i++) {
                    if (!ctx.docs.TryGetValue(siblings[i], out var sibling) ||
                        !sibling.stripped || sibling.prefabInstance != prefabInstanceId
                    ) {
                        continue;
                    }

                    siblingIndex = i;
                    break;
                }
            }

            segments.Add(new Segment(instance.instanceName, instance.sourcePrefabGuid, siblingIndex));
            return parent;
        }

        private static int GetSiblingIndex(FileContext ctx, long father, long transform) {
            if (father == 0) return ctx.roots?.IndexOf(transform) ?? (ctx.isPrefab ? 0 : -1);
            return ctx.docs.GetValueOrDefault(father)?.children?.IndexOf(transform) ?? -1;
        }

        private static Doc ParseHeader(string line) {
            var doc = new Doc { stripped = line.EndsWith(" stripped", StringComparison.Ordinal) };

            int classStart = "--- !u!".Length;
            int classEnd = line.IndexOf(' ', classStart);
            if (classEnd > classStart) int.TryParse(line.AsSpan(classStart, classEnd - classStart), out doc.classId);

            int idStart = line.IndexOf('&', classStart) + 1;
            if (idStart > 0) {
                int idEnd = line.IndexOf(' ', idStart);
                if (idEnd < 0) idEnd = line.Length;
                long.TryParse(line.AsSpan(idStart, idEnd - idStart), out doc.id);
            }

            return doc;
        }

        private static bool TryGetValue(string line, string prefix, out string value) {
            if (!line.StartsWith(prefix, StringComparison.Ordinal)) {
                value = null;
                return false;
            }

            value = line[prefix.Length..];
            return true;
        }

        private static long ParseFileId(string text) {
            const string key = "fileID: ";

            int start = text.IndexOf(key, StringComparison.Ordinal);
            if (start < 0) return 0;

            start += key.Length;
            int end = start;
            while (end < text.Length && (char.IsDigit(text[end]) || text[end] == '-')) end++;

            return long.TryParse(text.AsSpan(start, end - start), out long id) ? id : 0;
        }

        private static string ParseGuid(string text) {
            const string key = "guid: ";
            const int guidLength = 32;

            int start = text.IndexOf(key, StringComparison.Ordinal);
            if (start < 0) return null;

            start += key.Length;
            return start + guidLength <= text.Length ? text.Substring(start, guidLength) : null;
        }

        private static string Unquote(string value) {
            value = value.Trim();
            if (value.Length < 2) return value;

            if (value[0] == '\'' && value[^1] == '\'') return value[1..^1].Replace("''", "'");
            if (value[0] == '"' && value[^1] == '"') return value[1..^1].Replace("\\\"", "\"").Replace("\\\\", "\\");

            return value;
        }

        private sealed class Postprocessor : AssetPostprocessor {

            private static void OnPostprocessAllAssets(
                string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths
            ) {
                if (!_isStarted) return;

                for (int i = 0; i < deletedAssets.Length; i++) {
                    if (IsSearchTarget(deletedAssets[i])) Remove(deletedAssets[i]);
                }

                for (int i = 0; i < movedFromAssetPaths.Length; i++) {
                    if (IsSearchTarget(movedFromAssetPaths[i])) Remove(movedFromAssetPaths[i]);
                }

                for (int i = 0; i < importedAssets.Length; i++) {
                    if (IsSearchTarget(importedAssets[i])) Enqueue(importedAssets[i]);
                }

                for (int i = 0; i < movedAssets.Length; i++) {
                    if (IsSearchTarget(movedAssets[i])) Enqueue(movedAssets[i]);
                }
            }
        }
    }

}
