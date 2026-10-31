using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Common.Editor.Search {

    public sealed class AssetFileScanner {

        public const string KindAsset = "Asset";
        public const string KindPrefab = "Prefab";
        public const string KindScene = "Scene";

        private const string SceneExtension = ".unity";
        private const string PrefabExtension = ".prefab";
        private const string YamlHeader = "%YAML";
        private const string ReferencesPrefix = "references.RefIds.Array.data[";
        private const string ManagedReferencesPrefix = "managedReferences[";
        private const string LayerPropertyPath = "m_Layer";
        private const string NamePropertyPath = "m_Name";
        private const int MaxPrefabDepth = 64;

        private const int ClassIdGameObject = 1;
        private const int ClassIdTransform = 4;
        private const int ClassIdMonoBehaviour = 114;
        private const int ClassIdRectTransform = 224;
        private const int ClassIdPrefabInstance = 1001;

        public sealed class Request {

            public readonly List<(string assetPath, string kind)> files = new();
            public readonly HashSet<string> fieldNames = new();
            public readonly HashSet<string> referencedGuids = new();

            public void AddFiles(string[] searchFolders, bool assets, bool prefabs, bool scenes) {
                var added = new HashSet<string>();
                for (int i = 0; i < files.Count; i++) {
                    added.Add(files[i].assetPath);
                }

                if (searchFolders == null || searchFolders.Length == 0) return;

                if (assets) AddFiles("t:ScriptableObject", searchFolders, KindAsset, added);
                if (prefabs) AddFiles("t:Prefab", searchFolders, KindPrefab, added);
                if (scenes) AddFiles("t:Scene", searchFolders, KindScene, added);
            }

            public void AddOpenedScenes() {
                for (int i = 0; i < SceneManager.sceneCount; i++) {
                    var scene = SceneManager.GetSceneAt(i);
                    if (!scene.IsValid() || string.IsNullOrEmpty(scene.path)) continue;

                    bool contains = false;
                    for (int j = 0; j < files.Count && !contains; j++) {
                        contains = files[j].assetPath == scene.path;
                    }

                    if (!contains) files.Add((scene.path, KindScene));
                }
            }

            private void AddFiles(string filter, string[] searchFolders, string kind, HashSet<string> added) {
                string[] guids = AssetDatabase.FindAssets(filter, searchFolders);

                for (int i = 0; i < guids.Length; i++) {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (string.IsNullOrEmpty(path) || !IsKind(path, kind) || !added.Add(path)) continue;

                    files.Add((path, kind));
                }
            }

            private static bool IsKind(string path, string kind) {
                bool isScene = path.EndsWith(SceneExtension, StringComparison.OrdinalIgnoreCase);
                bool isPrefab = path.EndsWith(PrefabExtension, StringComparison.OrdinalIgnoreCase);

                return kind switch {
                    KindScene => isScene,
                    KindPrefab => isPrefab,
                    _ => !isScene && !isPrefab && path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase),
                };
            }
        }

        public sealed class ScannedFile {

            public readonly string assetPath;
            public readonly string kind;
            public readonly List<ScannedObject> objects = new();
            public readonly List<ScannedInstance> instances = new();

            public ScannedFile(string assetPath, string kind) {
                this.assetPath = assetPath;
                this.kind = kind;
            }
        }

        public sealed class ScannedObject {

            public readonly string path;
            public readonly bool isGameObject;
            public readonly int layer;
            public readonly string scriptGuid;
            public readonly IReadOnlyList<Field> fields;

            public ScannedObject(string path, bool isGameObject, int layer, string scriptGuid, IReadOnlyList<Field> fields) {
                this.path = path;
                this.isGameObject = isGameObject;
                this.layer = layer;
                this.scriptGuid = scriptGuid;
                this.fields = fields;
            }
        }

        public readonly struct ScannedInstance {

            public readonly string path;
            public readonly string sourcePrefabGuid;
            public readonly string nestedInPrefabGuid;

            public ScannedInstance(string path, string sourcePrefabGuid, string nestedInPrefabGuid) {
                this.path = path;
                this.sourcePrefabGuid = sourcePrefabGuid;
                this.nestedInPrefabGuid = nestedInPrefabGuid;
            }
        }

        public readonly struct Field {

            public readonly string path;
            public readonly string value;
            public readonly string refGuid;
            public readonly long refFileId;
            public readonly string typeKey;
            public readonly string relativePath;

            public Field(string path, string value, string refGuid, long refFileId, string typeKey, string relativePath) {
                this.path = path;
                this.value = value;
                this.refGuid = refGuid;
                this.refFileId = refFileId;
                this.typeKey = typeKey;
                this.relativePath = relativePath;
            }
        }

        public readonly struct ReferenceSlot {

            public readonly ScannedFile file;
            public readonly ScannedObject owner;
            public readonly string propertyPath;
            public readonly string refGuid;
            public readonly int id;

            public ReferenceSlot(ScannedFile file, ScannedObject owner, string propertyPath, string refGuid, int id) {
                this.file = file;
                this.owner = owner;
                this.propertyPath = propertyPath;
                this.refGuid = refGuid;
                this.id = id;
            }
        }

        public bool IsDone => _task == null || _task.IsCompleted;
        public bool IsCancelled => _cts.IsCancellationRequested;
        public int ProcessedCount => _processedCount;
        public int TotalCount => _totalCount;
        public IReadOnlyList<ScannedFile> Files => _files;

        private static readonly List<AssetFileScanner> _activeScanners = new();
        private static readonly Dictionary<string, Type> _scriptTypes = new();
        private static readonly Dictionary<string, Type> _managedTypes = new();

        private readonly CancellationTokenSource _cts = new();
        private readonly List<(string assetPath, string kind)> _targets;
        private readonly HashSet<string> _fieldNames;
        private readonly HashSet<string> _referencedGuids;
        private readonly Dictionary<string, string> _prefabPathByGuid;
        private readonly Dictionary<string, string> _guidByPath;
        private readonly string _projectFolder;
        private readonly ConcurrentDictionary<string, RawFile> _rawFiles = new();
        private readonly Dictionary<string, ResolvedFile> _resolvedFiles = new();
        private readonly HashSet<string> _resolveStack = new();
        private readonly List<ScannedFile> _files = new();
        private Task _task;
        private int _processedCount;
        private int _totalCount;

        static AssetFileScanner() {
            AssemblyReloadEvents.beforeAssemblyReload += CancelAll;
            EditorApplication.quitting += CancelAll;
        }

        private AssetFileScanner(Request request) {
            _targets = new List<(string, string)>(request.files);
            _fieldNames = new HashSet<string>(request.fieldNames);
            _referencedGuids = new HashSet<string>(request.referencedGuids);
            _projectFolder = Directory.GetParent(Application.dataPath)!.FullName;
            _prefabPathByGuid = new Dictionary<string, string>();
            _guidByPath = new Dictionary<string, string>();

            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.EndsWith(PrefabExtension, StringComparison.OrdinalIgnoreCase)) continue;

                _prefabPathByGuid[guids[i]] = path;
                _guidByPath[path] = guids[i];
            }

            for (int i = 0; i < _targets.Count; i++) {
                string path = _targets[i].assetPath;
                if (!_guidByPath.ContainsKey(path)) _guidByPath[path] = AssetDatabase.AssetPathToGUID(path);
            }
        }

        public static AssetFileScanner Start(Request request) {
            var scanner = new AssetFileScanner(request);

            _activeScanners.Add(scanner);
            scanner._task = Task.Run(scanner.Run);

            return scanner;
        }

        public static bool SaveOpenedScenesIfUserWantsTo() {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                if (SceneManager.GetSceneAt(i).isDirty) return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            return true;
        }

        public void Cancel() {
            if (!_cts.IsCancellationRequested) _cts.Cancel();
            _activeScanners.Remove(this);
        }

        public void Complete() {
            _activeScanners.Remove(this);
        }

        public static void CollectReferenceSlots(
            IReadOnlyList<ScannedFile> files,
            string refFieldName,
            string idFieldName,
            Func<Type, bool> parentTypeFilter,
            List<ReferenceSlot> result
        ) {
            var refFields = new Dictionary<string, Field>();
            var idFields = new Dictionary<string, Field>();
            var typeChecks = new Dictionary<(string, string), bool>();

            for (int f = 0; f < files.Count; f++) {
                var file = files[f];

                for (int o = 0; o < file.objects.Count; o++) {
                    var obj = file.objects[o];
                    if (obj.fields == null || obj.fields.Count == 0) continue;

                    refFields.Clear();
                    idFields.Clear();

                    for (int i = 0; i < obj.fields.Count; i++) {
                        var field = obj.fields[i];
                        string name = GetLastSegment(field.path, out string parentPath);

                        if (name == refFieldName) refFields[parentPath] = field;
                        else if (name == idFieldName) idFields[parentPath] = field;
                    }

                    foreach ((string parentPath, var refField) in refFields) {
                        if (string.IsNullOrEmpty(refField.refGuid)) continue;

                        if (parentTypeFilter != null) {
                            GetLastSegment(refField.relativePath, out string parentRelativePath);
                            var key = (refField.typeKey, parentRelativePath);

                            if (!typeChecks.TryGetValue(key, out bool isValidType)) {
                                var type = ResolveType(refField.typeKey, parentRelativePath);
                                isValidType = type == null || parentTypeFilter.Invoke(type);
                                typeChecks[key] = isValidType;
                            }

                            if (!isValidType) continue;
                        }

                        int id = idFields.TryGetValue(parentPath, out var idField) && int.TryParse(idField.value, out int value) ? value : 0;
                        result.Add(new ReferenceSlot(file, obj, refField.path, refField.refGuid, id));
                    }
                }
            }
        }

        public static string GetScriptTypeName(string scriptGuid) {
            return GetScriptType(scriptGuid)?.FullName;
        }

        public static Type GetScriptType(string scriptGuid) {
            if (string.IsNullOrEmpty(scriptGuid)) return null;
            if (_scriptTypes.TryGetValue(scriptGuid, out var type)) return type;

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(scriptGuid));
            type = script != null ? script.GetClass() : null;
            _scriptTypes[scriptGuid] = type;

            return type;
        }

        private static Type ResolveType(string typeKey, string relativePath) {
            var type = typeKey switch {
                null => null,
                _ when typeKey.StartsWith("s:", StringComparison.Ordinal) => GetScriptType(typeKey[2..]),
                _ when typeKey.StartsWith("r:", StringComparison.Ordinal) => GetManagedType(typeKey[2..]),
                _ => null,
            };

            if (type == null || string.IsNullOrEmpty(relativePath)) return type;

            string[] segments = relativePath.Split('.');

            for (int i = 0; i < segments.Length && type != null; i++) {
                if (segments[i] == "Array" && i + 1 < segments.Length && segments[i + 1].StartsWith("data[", StringComparison.Ordinal)) {
                    type = GetElementType(type);
                    i++;
                    continue;
                }

                type = FindField(type, segments[i])?.FieldType;
            }

            return type;
        }

        private static Type GetManagedType(string typeText) {
            if (_managedTypes.TryGetValue(typeText, out var type)) return type;

            string className = GetFlowValue(typeText, "class");
            string ns = GetFlowValue(typeText, "ns");
            string asm = GetFlowValue(typeText, "asm");

            if (!string.IsNullOrEmpty(className)) {
                string fullName = string.IsNullOrEmpty(ns) ? className.Replace('/', '+') : $"{ns}.{className.Replace('/', '+')}";
                type = Type.GetType(string.IsNullOrEmpty(asm) ? fullName : $"{fullName}, {asm}");
            }

            _managedTypes[typeText] = type;
            return type;
        }

        private static string GetFlowValue(string text, string key) {
            string prefix = key + ": ";
            int start = text.IndexOf(prefix, StringComparison.Ordinal);
            if (start < 0) return null;

            start += prefix.Length;
            int end = start;
            while (end < text.Length && text[end] != ',' && text[end] != '}') end++;

            return text[start..end].Trim();
        }

        private static Type GetElementType(Type type) {
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return type.GetGenericArguments()[0];
            return null;
        }

        private static FieldInfo FindField(Type type, string name) {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (var t = type; t != null; t = t.BaseType) {
                var field = t.GetField(name, flags);
                if (field != null) return field;
            }

            return null;
        }

        private static string GetLastSegment(string path, out string parentPath) {
            int dot = path.LastIndexOf('.');
            parentPath = dot < 0 ? string.Empty : path[..dot];
            return dot < 0 ? path : path[(dot + 1)..];
        }

        private static void CancelAll() {
            for (int i = _activeScanners.Count - 1; i >= 0; i--) {
                _activeScanners[i].Cancel();
            }
        }

        private void Run() {
            var token = _cts.Token;
            _totalCount = _targets.Count;

            try {
                var paths = new List<string>(_targets.Count);
                for (int i = 0; i < _targets.Count; i++) {
                    paths.Add(_targets[i].assetPath);
                }

                while (paths.Count > 0 && !token.IsCancellationRequested) {
                    Parallel.ForEach(paths, new ParallelOptions { CancellationToken = token }, ParseAndStore);
                    paths = CollectMissingDependencies();
                    Interlocked.Add(ref _totalCount, paths.Count);
                }

                for (int i = 0; i < _targets.Count && !token.IsCancellationRequested; i++) {
                    (string assetPath, string kind) = _targets[i];
                    _files.Add(CreateScannedFile(assetPath, kind, Resolve(assetPath, depth: 0)));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }

        private void ParseAndStore(string assetPath) {
            RawFile file = null;

            try {
                string text = File.ReadAllText(Path.Combine(_projectFolder, assetPath));
                if (text.StartsWith(YamlHeader, StringComparison.Ordinal)) file = Parse(text, _fieldNames, _referencedGuids, _guidByPath.GetValueOrDefault(assetPath));
            }
            catch (Exception) {
                file = null;
            }

            _rawFiles[assetPath] = file ?? new RawFile();
            Interlocked.Increment(ref _processedCount);
        }

        private List<string> CollectMissingDependencies() {
            var missing = new HashSet<string>();

            foreach (var file in _rawFiles.Values) {
                for (int i = 0; i < file.instances.Count; i++) {
                    string guid = file.instances[i].sourcePrefabGuid;

                    if (guid != null &&
                        _prefabPathByGuid.TryGetValue(guid, out string path) &&
                        !_rawFiles.ContainsKey(path))
                    {
                        missing.Add(path);
                    }
                }
            }

            return new List<string>(missing);
        }

        private ScannedFile CreateScannedFile(string assetPath, string kind, ResolvedFile resolved) {
            var file = new ScannedFile(assetPath, kind);

            for (int i = 0; i < resolved.objects.Count; i++) {
                var obj = resolved.objects[i];
                if (kind == KindAsset && (obj.isGameObject || (obj.hideFlags & (int) HideFlags.HideInHierarchy) != 0)) continue;

                string path = kind == KindAsset ? obj.name : string.Join("/", obj.segments);
                file.objects.Add(new ScannedObject(path, obj.isGameObject, obj.layer, obj.scriptGuid, obj.fields));
            }

            for (int i = 0; i < resolved.instances.Count; i++) {
                var instance = resolved.instances[i];
                file.instances.Add(new ScannedInstance(string.Join("/", instance.segments), instance.sourcePrefabGuid, instance.nestedInPrefabGuid));
            }

            return file;
        }

        private sealed class RawFile {
            public readonly Dictionary<long, RawDoc> docs = new();
            public readonly Dictionary<long, long> transformByGameObject = new();
            public readonly List<RawDoc> instances = new();
        }

        private sealed class RawDoc {
            public int classId;
            public long id;
            public bool stripped;
            public string name;
            public int layer;
            public int hideFlags;
            public long gameObject;
            public long father;
            public long prefabInstance;
            public long sourceId;
            public long transformParent;
            public string scriptGuid;
            public string sourcePrefabGuid;
            public List<Modification> modifications;
            public HashSet<long> removed;
            public List<Field> fields;
            public Dictionary<long, (string path, string type)> refs;
        }

        private readonly struct Modification {

            public readonly long targetId;
            public readonly string targetGuid;
            public readonly string propertyPath;
            public readonly string value;
            public readonly string refGuid;
            public readonly long refFileId;

            public Modification(long targetId, string targetGuid, string propertyPath, string value, string refGuid, long refFileId) {
                this.targetId = targetId;
                this.targetGuid = targetGuid;
                this.propertyPath = propertyPath;
                this.value = value;
                this.refGuid = refGuid;
                this.refFileId = refFileId;
            }
        }

        private sealed class ResolvedFile {
            public readonly List<ResolvedObject> objects = new();
            public readonly List<ResolvedInstance> instances = new();
            public readonly Dictionary<long, ResolvedObject> objectById = new();
            public readonly Dictionary<long, ResolvedObject> gameObjectByTransform = new();
            public long rootGameObjectId;
            public string rootName;
        }

        private sealed class ResolvedObject {
            public long id;
            public bool isGameObject;
            public long transformId;
            public long gameObjectId;
            public string[] segments;
            public string name;
            public int layer;
            public int hideFlags;
            public string scriptGuid;
            public List<Field> fields;
            public Dictionary<long, (string path, string type)> refs;
        }

        private sealed class ResolvedInstance {
            public string[] segments;
            public string sourcePrefabGuid;
            public string nestedInPrefabGuid;
        }

        private sealed class ResolveContext {
            public RawFile raw;
            public ResolvedFile result;
            public readonly Dictionary<long, (string[] segments, ResolvedFile source, long id)> instances = new();
            public readonly Dictionary<long, string[]> transformPaths = new();
            public readonly HashSet<long> pending = new();
            public int depth;
        }

        private static readonly ResolvedFile EmptyFile = new();
        private static readonly string[] EmptySegments = Array.Empty<string>();

        private ResolvedFile Resolve(string assetPath, int depth) {
            if (_resolvedFiles.TryGetValue(assetPath, out var resolved)) return resolved;
            if (!_rawFiles.TryGetValue(assetPath, out var raw) || depth > MaxPrefabDepth || !_resolveStack.Add(assetPath)) return EmptyFile;

            resolved = new ResolvedFile();
            var ctx = new ResolveContext { raw = raw, result = resolved, depth = depth };

            foreach (var doc in raw.docs.Values) {
                if (doc.stripped) continue;

                switch (doc.classId) {
                    case ClassIdGameObject: {
                        long transform = raw.transformByGameObject.GetValueOrDefault(doc.id);
                        AddObject(resolved, new ResolvedObject {
                            id = doc.id,
                            isGameObject = true,
                            transformId = transform,
                            gameObjectId = doc.id,
                            segments = GetTransformPath(ctx, transform),
                            name = doc.name,
                            layer = doc.layer,
                        });
                        break;
                    }

                    case ClassIdMonoBehaviour:
                        AddObject(resolved, new ResolvedObject {
                            id = doc.id,
                            gameObjectId = doc.gameObject,
                            segments = doc.gameObject == 0 ? new[] { doc.name } : GetGameObjectPath(ctx, doc.gameObject),
                            name = doc.name,
                            hideFlags = doc.hideFlags,
                            scriptGuid = doc.scriptGuid,
                            fields = doc.fields,
                            refs = doc.refs,
                        });
                        break;
                }
            }

            for (int i = 0; i < raw.instances.Count; i++) {
                AddInstance(ctx, raw.instances[i]);
            }

            for (int i = 0; i < resolved.objects.Count; i++) {
                var obj = resolved.objects[i];
                if (!obj.isGameObject || obj.segments.Length != 1) continue;

                resolved.rootGameObjectId = obj.id;
                resolved.rootName = obj.segments[0];
                break;
            }

            _resolveStack.Remove(assetPath);
            _resolvedFiles[assetPath] = resolved;

            return resolved;
        }

        private static void AddObject(ResolvedFile file, ResolvedObject obj) {
            file.objects.Add(obj);
            file.objectById[obj.id] = obj;
            if (obj.isGameObject && obj.transformId != 0) file.gameObjectByTransform[obj.transformId] = obj;
        }

        private void AddInstance(ResolveContext ctx, RawDoc doc) {
            var (segments, source, instanceId) = GetInstance(ctx, doc.id);
            if (source == null) return;

            var file = ctx.result;
            var modsByTarget = new Dictionary<long, List<Modification>>();

            if (doc.modifications != null) {
                for (int i = 0; i < doc.modifications.Count; i++) {
                    var mod = doc.modifications[i];
                    if (mod.targetGuid != doc.sourcePrefabGuid) continue;

                    if (!modsByTarget.TryGetValue(mod.targetId, out var list)) {
                        list = new List<Modification>();
                        modsByTarget[mod.targetId] = list;
                    }

                    list.Add(mod);
                }
            }

            for (int i = 0; i < source.objects.Count; i++) {
                var src = source.objects[i];
                if (doc.removed != null && (doc.removed.Contains(src.id) || doc.removed.Contains(src.gameObjectId))) continue;

                var obj = new ResolvedObject {
                    id = CombineId(src.id, instanceId),
                    isGameObject = src.isGameObject,
                    transformId = src.transformId == 0 ? 0 : CombineId(src.transformId, instanceId),
                    gameObjectId = src.gameObjectId == 0 ? 0 : CombineId(src.gameObjectId, instanceId),
                    segments = ConcatSegments(segments, src.segments),
                    name = src.name,
                    layer = src.layer,
                    hideFlags = src.hideFlags,
                    scriptGuid = src.scriptGuid,
                    fields = src.fields,
                    refs = src.refs,
                };

                if (modsByTarget.TryGetValue(src.id, out var mods)) ApplyModifications(obj, mods);

                AddObject(file, obj);
            }

            file.instances.Add(new ResolvedInstance {
                segments = segments,
                sourcePrefabGuid = doc.sourcePrefabGuid,
            });

            for (int i = 0; i < source.instances.Count; i++) {
                var nested = source.instances[i];

                file.instances.Add(new ResolvedInstance {
                    segments = ConcatSegments(segments, nested.segments),
                    sourcePrefabGuid = nested.sourcePrefabGuid,
                    nestedInPrefabGuid = doc.sourcePrefabGuid,
                });
            }
        }

        private void ApplyModifications(ResolvedObject obj, List<Modification> mods) {
            List<Field> fields = null;

            for (int i = 0; i < mods.Count; i++) {
                var mod = mods[i];

                if (mod.propertyPath == LayerPropertyPath) {
                    if (int.TryParse(mod.value, out int layer)) obj.layer = layer;
                    continue;
                }

                if (mod.propertyPath == NamePropertyPath || obj.isGameObject) continue;

                string path;
                string typeKey;
                string relativePath;

                if (mod.propertyPath.StartsWith(ManagedReferencesPrefix, StringComparison.Ordinal)) {
                    int end = mod.propertyPath.IndexOf(']');
                    if (end < 0 || end + 2 > mod.propertyPath.Length ||
                        !long.TryParse(mod.propertyPath.AsSpan(ManagedReferencesPrefix.Length, end - ManagedReferencesPrefix.Length), out long rid) ||
                        obj.refs == null || !obj.refs.TryGetValue(rid, out var reference))
                    {
                        continue;
                    }

                    relativePath = mod.propertyPath[(end + 2)..];
                    path = $"{reference.path}.{relativePath}";
                    typeKey = "r:" + reference.type;
                }
                else {
                    path = mod.propertyPath;
                    relativePath = path;
                    typeKey = "s:" + obj.scriptGuid;
                }

                int index = (fields ?? obj.fields)?.FindIndex(f => f.path == path) ?? -1;

                if (index < 0 &&
                    !_fieldNames.Contains(GetLastSegment(path, out _)) &&
                    (mod.refGuid == null || !_referencedGuids.Contains(mod.refGuid)))
                {
                    continue;
                }

                fields ??= obj.fields == null ? new List<Field>() : new List<Field>(obj.fields);
                var field = new Field(path, mod.value, mod.refGuid, mod.refFileId, typeKey, relativePath);

                if (index >= 0) fields[index] = field;
                else fields.Add(field);
            }

            if (fields != null) obj.fields = fields;
        }

        private (string[] segments, ResolvedFile source, long id) GetInstance(ResolveContext ctx, long instanceId) {
            if (ctx.instances.TryGetValue(instanceId, out var instance)) return instance;
            if (!ctx.raw.docs.TryGetValue(instanceId, out var doc) || !ctx.pending.Add(instanceId)) return (EmptySegments, null, instanceId);

            var source = doc.sourcePrefabGuid != null && _prefabPathByGuid.TryGetValue(doc.sourcePrefabGuid, out string sourcePath)
                ? Resolve(sourcePath, ctx.depth + 1)
                : EmptyFile;

            string name = null;

            if (doc.modifications != null) {
                for (int i = 0; i < doc.modifications.Count; i++) {
                    var mod = doc.modifications[i];
                    if (mod.propertyPath != NamePropertyPath || (source != EmptyFile && mod.targetId != source.rootGameObjectId)) continue;

                    name = Unquote(mod.value);
                    break;
                }
            }

            name ??= source.rootName ?? (doc.sourcePrefabGuid != null && _prefabPathByGuid.TryGetValue(doc.sourcePrefabGuid, out string p)
                ? Path.GetFileNameWithoutExtension(p)
                : "?");

            var parent = doc.transformParent == 0 ? EmptySegments : GetTransformPath(ctx, doc.transformParent);
            var segments = new string[parent.Length + 1];
            Array.Copy(parent, segments, parent.Length);
            segments[^1] = name;

            instance = (segments, source, instanceId);
            ctx.instances[instanceId] = instance;
            ctx.pending.Remove(instanceId);

            return instance;
        }

        private string[] GetGameObjectPath(ResolveContext ctx, long gameObjectId) {
            if (!ctx.raw.docs.TryGetValue(gameObjectId, out var doc)) return EmptySegments;

            if (doc.stripped) {
                var (segments, source, _) = GetInstance(ctx, doc.prefabInstance);
                return source != null && source.objectById.TryGetValue(doc.sourceId, out var src)
                    ? ConcatSegments(segments, src.segments)
                    : segments;
            }

            return GetTransformPath(ctx, ctx.raw.transformByGameObject.GetValueOrDefault(gameObjectId));
        }

        private string[] GetTransformPath(ResolveContext ctx, long transformId) {
            if (transformId == 0) return EmptySegments;
            if (ctx.transformPaths.TryGetValue(transformId, out var path)) return path;
            if (!ctx.raw.docs.TryGetValue(transformId, out var doc)) return EmptySegments;

            if (doc.stripped) {
                var (segments, source, _) = GetInstance(ctx, doc.prefabInstance);
                path = source != null && source.gameObjectByTransform.TryGetValue(doc.sourceId, out var src)
                    ? ConcatSegments(segments, src.segments)
                    : segments;
            }
            else {
                ctx.transformPaths[transformId] = EmptySegments;

                var parent = GetTransformPath(ctx, doc.father);
                string name = ctx.raw.docs.TryGetValue(doc.gameObject, out var gameObject) ? gameObject.name : "?";

                path = new string[parent.Length + 1];
                Array.Copy(parent, path, parent.Length);
                path[^1] = name;
            }

            ctx.transformPaths[transformId] = path;
            return path;
        }

        private static string[] ConcatSegments(string[] instanceSegments, string[] sourceSegments) {
            int sourceCount = Math.Max(0, sourceSegments.Length - 1);
            var segments = new string[instanceSegments.Length + sourceCount];

            Array.Copy(instanceSegments, segments, instanceSegments.Length);
            if (sourceCount > 0) Array.Copy(sourceSegments, 1, segments, instanceSegments.Length, sourceCount);

            return segments;
        }

        private static long CombineId(long sourceId, long instanceId) {
            return (sourceId ^ instanceId) & long.MaxValue;
        }

        private static RawFile Parse(string text, HashSet<string> fieldNames, HashSet<string> referencedGuids, string fileGuid) {
            var file = new RawFile();
            bool parseFields = fieldNames.Count > 0 || referencedGuids.Count > 0;
            bool trackAllModifications = referencedGuids.Count > 0;

            RawDoc doc = null;
            FieldParser fieldParser = null;
            List<long> removedList = null;
            long modTargetId = 0;
            string modTargetGuid = null;
            string modPath = null;
            string modValue = null;

            using var reader = new StringReader(text);

            while (reader.ReadLine() is { } line) {
                if (line.StartsWith("--- !u!", StringComparison.Ordinal)) {
                    fieldParser?.Finish();
                    fieldParser = null;
                    removedList = null;
                    modPath = null;

                    doc = ParseHeader(line);
                    file.docs[doc.id] = doc;

                    if (doc.classId == ClassIdPrefabInstance) file.instances.Add(doc);
                    if (doc.classId == ClassIdMonoBehaviour && !doc.stripped && parseFields) fieldParser = new FieldParser(fieldNames, referencedGuids, fileGuid, doc);
                    continue;
                }

                if (doc == null) continue;

                if (fieldParser != null) {
                    fieldParser.Line(line);
                    continue;
                }

                switch (doc.classId) {
                    case ClassIdGameObject:
                        if (TryGetValue(line, "  m_Name: ", out string name)) doc.name = Unquote(name);
                        else if (TryGetValue(line, "  m_Layer: ", out string layer)) int.TryParse(layer, out doc.layer);
                        else if (TryGetValue(line, "  m_PrefabInstance: ", out string prefabInstance)) doc.prefabInstance = ParseFileId(prefabInstance);
                        else if (TryGetValue(line, "  m_CorrespondingSourceObject: ", out string source)) doc.sourceId = ParseFileId(source);
                        break;

                    case ClassIdTransform:
                    case ClassIdRectTransform:
                        if (TryGetValue(line, "  m_GameObject: ", out string gameObject)) doc.gameObject = ParseFileId(gameObject);
                        else if (TryGetValue(line, "  m_Father: ", out string father)) doc.father = ParseFileId(father);
                        else if (TryGetValue(line, "  m_PrefabInstance: ", out string prefabInstance)) doc.prefabInstance = ParseFileId(prefabInstance);
                        else if (TryGetValue(line, "  m_CorrespondingSourceObject: ", out string source)) doc.sourceId = ParseFileId(source);
                        break;

                    case ClassIdPrefabInstance:
                        if (removedList != null) {
                            if (line.StartsWith("    - {fileID: ", StringComparison.Ordinal)) {
                                (doc.removed ??= new HashSet<long>()).Add(ParseFileId(line));
                                continue;
                            }

                            removedList = null;
                        }

                        if (TryGetValue(line, "    m_TransformParent: ", out string transformParent)) {
                            doc.transformParent = ParseFileId(transformParent);
                        }
                        else if (TryGetValue(line, "  m_SourcePrefab: ", out string sourcePrefab)) {
                            doc.sourcePrefabGuid = ParseGuid(sourcePrefab);
                        }
                        else if (line.StartsWith("    m_RemovedComponents:", StringComparison.Ordinal) ||
                                 line.StartsWith("    m_RemovedGameObjects:", StringComparison.Ordinal))
                        {
                            removedList = new List<long>();
                        }
                        else if (TryGetValue(line, "    - target: ", out string target)) {
                            modTargetId = ParseFileId(target);
                            modTargetGuid = ParseGuid(target);
                            modPath = null;
                            modValue = null;
                        }
                        else if (TryGetValue(line, "      propertyPath: ", out string propertyPath)) {
                            modPath = propertyPath.Trim();
                        }
                        else if (modPath != null && TryGetValue(line, "      value: ", out string value)) {
                            modValue = value.Trim();
                        }
                        else if (modPath != null && TryGetValue(line, "      objectReference: ", out string objectReference)) {
                            if (trackAllModifications || IsTrackedModification(modPath, fieldNames)) {
                                long refFileId = ParseFileId(objectReference);
                                string refGuid = refFileId == 0 ? null : ParseGuid(objectReference) ?? fileGuid;

                                (doc.modifications ??= new List<Modification>())
                                    .Add(new Modification(modTargetId, modTargetGuid, modPath, modValue ?? string.Empty, refGuid, refFileId));
                            }

                            modPath = null;
                        }
                        break;
                }
            }

            fieldParser?.Finish();

            foreach (var d in file.docs.Values) {
                if (d.stripped || d.gameObject == 0 || d.classId is not (ClassIdTransform or ClassIdRectTransform)) continue;
                file.transformByGameObject[d.gameObject] = d.id;
            }

            return file;
        }

        private static bool IsTrackedModification(string propertyPath, HashSet<string> fieldNames) {
            if (propertyPath == LayerPropertyPath || propertyPath == NamePropertyPath) return true;
            if (fieldNames.Count == 0) return false;

            return fieldNames.Contains(GetLastSegment(propertyPath, out _));
        }

        private sealed class FieldParser {

            private readonly HashSet<string> _fieldNames;
            private readonly HashSet<string> _referencedGuids;
            private readonly string _fileGuid;
            private readonly RawDoc _doc;
            private readonly List<(int indent, string path)> _stack = new();
            private readonly Dictionary<int, string> _lastEmptyKeyByIndent = new();
            private readonly Dictionary<string, int> _listCounts = new();
            private readonly List<(string path, string value)> _fields = new();
            private readonly List<(string path, long rid)> _rids = new();
            private readonly Dictionary<string, string> _refTypes = new();
            private string _pendingPath;

            public FieldParser(HashSet<string> fieldNames, HashSet<string> referencedGuids, string fileGuid, RawDoc doc) {
                _fieldNames = fieldNames;
                _referencedGuids = referencedGuids;
                _fileGuid = fileGuid;
                _doc = doc;
            }

            public void Line(string line) {
                int indent = 0;
                while (indent < line.Length && line[indent] == ' ') indent++;

                if (indent == 0 || indent >= line.Length) return;

                if (line[indent] == '-' && (indent + 1 == line.Length || line[indent + 1] == ' ')) {
                    Pop(indent);
                    if (!_lastEmptyKeyByIndent.TryGetValue(indent, out string owner)) return;

                    int index = _listCounts.GetValueOrDefault(owner);
                    _listCounts[owner] = index + 1;

                    string itemPath = $"{owner}.Array.data[{index}]";
                    _stack.Add((indent + 2, itemPath));

                    int start = indent + 2;
                    if (start >= line.Length) return;

                    if (TryParseKey(line, start, out string itemKey, out string itemValue)) HandleKey(start, itemKey, itemValue);
                    else Leaf(itemPath, "data", line[start..]);

                    return;
                }

                if (TryParseKey(line, indent, out string key, out string value)) HandleKey(indent, key, value);
            }

            public void Finish() {
                var doc = _doc;

                var items = new Dictionary<string, (long rid, string type)>();

                for (int i = 0; i < _rids.Count; i++) {
                    (string path, long rid) = _rids[i];
                    if (!path.StartsWith(ReferencesPrefix, StringComparison.Ordinal)) continue;

                    string itemPrefix = GetItemPrefix(path);
                    if (itemPrefix != path) continue;

                    items[itemPrefix] = (rid, _refTypes.GetValueOrDefault(itemPrefix));
                }

                var parents = new Dictionary<long, (long parentRid, string relativePath)>();

                for (int i = 0; i < _rids.Count; i++) {
                    (string path, long rid) = _rids[i];
                    if (rid < 0) continue;

                    if (!path.StartsWith(ReferencesPrefix, StringComparison.Ordinal)) {
                        parents.TryAdd(rid, (-1, path));
                        continue;
                    }

                    string itemPrefix = GetItemPrefix(path);
                    string dataPrefix = itemPrefix + ".data.";

                    if (path.StartsWith(dataPrefix, StringComparison.Ordinal) && items.TryGetValue(itemPrefix, out var item)) {
                        parents.TryAdd(rid, (item.rid, path[dataPrefix.Length..]));
                    }
                }

                var refPaths = new Dictionary<long, string>();
                doc.refs = new Dictionary<long, (string path, string type)>();

                foreach (var item in items.Values) {
                    doc.refs[item.rid] = (GetRefPath(item.rid, parents, refPaths, 0), item.type);
                }

                doc.fields = new List<Field>(_fields.Count);

                for (int i = 0; i < _fields.Count; i++) {
                    (string path, string value) = _fields[i];
                    long refFileId = GetReference(value, out string refGuid);

                    if (!path.StartsWith(ReferencesPrefix, StringComparison.Ordinal)) {
                        doc.fields.Add(new Field(path, value, refGuid, refFileId, "s:" + doc.scriptGuid, path));
                        continue;
                    }

                    string itemPrefix = GetItemPrefix(path);
                    string dataPrefix = itemPrefix + ".data.";
                    if (!path.StartsWith(dataPrefix, StringComparison.Ordinal) || !items.TryGetValue(itemPrefix, out var item)) continue;

                    string relativePath = path[dataPrefix.Length..];
                    string refPath = doc.refs[item.rid].path;

                    doc.fields.Add(new Field($"{refPath}.{relativePath}", value, refGuid, refFileId, "r:" + item.type, relativePath));
                }
            }

            private static string GetRefPath(
                long rid,
                Dictionary<long, (long parentRid, string relativePath)> parents,
                Dictionary<long, string> refPaths,
                int depth
            ) {
                if (refPaths.TryGetValue(rid, out string path)) return path;

                if (depth > MaxPrefabDepth || !parents.TryGetValue(rid, out var parent)) {
                    path = $"{ManagedReferencesPrefix}{rid}]";
                }
                else {
                    refPaths[rid] = $"{ManagedReferencesPrefix}{rid}]";
                    path = parent.parentRid < 0
                        ? parent.relativePath
                        : $"{GetRefPath(parent.parentRid, parents, refPaths, depth + 1)}.{parent.relativePath}";
                }

                refPaths[rid] = path;
                return path;
            }

            private static string GetItemPrefix(string path) {
                int end = path.IndexOf(']', ReferencesPrefix.Length);
                return end < 0 ? path : path[..(end + 1)];
            }

            private void HandleKey(int indent, string key, string value) {
                Pop(indent);

                if (_stack.Count == 0 || _stack[^1].indent < indent) {
                    _stack.Add((indent, _stack.Count == 0 ? string.Empty : _pendingPath ?? string.Empty));
                }

                string parent = _stack[^1].path;
                string path = parent.Length == 0 ? key : $"{parent}.{key}";

                if (value.Length == 0) {
                    _lastEmptyKeyByIndent[indent] = path;
                    _pendingPath = path;
                    return;
                }

                Leaf(path, key, value);
            }

            private void Leaf(string path, string key, string value) {
                GetLastSegment(path, out string parentPath);

                if (parentPath.Length == 0) {
                    switch (key) {
                        case "m_GameObject":
                            _doc.gameObject = ParseFileId(value);
                            return;

                        case "m_Script":
                            _doc.scriptGuid = ParseGuid(value);
                            return;

                        case NamePropertyPath:
                            _doc.name = Unquote(value);
                            return;

                        case "m_ObjectHideFlags":
                            int.TryParse(value, out _doc.hideFlags);
                            return;
                    }
                }

                if (key == "rid") {
                    if (parentPath.Length > 0 && long.TryParse(value, out long rid)) _rids.Add((parentPath, rid));
                    return;
                }

                if (key == "type" && path.StartsWith(ReferencesPrefix, StringComparison.Ordinal)) {
                    _refTypes[parentPath] = value;
                    return;
                }

                if (_fieldNames.Contains(key) ||
                    _referencedGuids.Count > 0 && GetReference(value, out string refGuid) != 0 && _referencedGuids.Contains(refGuid))
                {
                    _fields.Add((path, value));
                }
            }

            private long GetReference(string value, out string refGuid) {
                refGuid = null;
                if (!value.StartsWith("{fileID:", StringComparison.Ordinal)) return 0;

                long fileId = ParseFileId(value);
                if (fileId != 0) refGuid = ParseGuid(value) ?? _fileGuid;

                return fileId;
            }

            private void Pop(int indent) {
                while (_stack.Count > 0 && _stack[^1].indent > indent) {
                    _stack.RemoveAt(_stack.Count - 1);
                }
            }

            private static bool TryParseKey(string line, int start, out string key, out string value) {
                int i = start;

                while (i < line.Length && IsKeyChar(line[i])) i++;

                if (i == start || i >= line.Length || line[i] != ':' || (i + 1 < line.Length && line[i + 1] != ' ')) {
                    key = null;
                    value = null;
                    return false;
                }

                key = line[start..i];
                value = i + 2 <= line.Length ? line[(i + 1)..].Trim() : string.Empty;
                return true;
            }

            private static bool IsKeyChar(char c) {
                return char.IsLetterOrDigit(c) || c == '_' || c == '<' || c == '>';
            }
        }

        private static RawDoc ParseHeader(string line) {
            var doc = new RawDoc { stripped = line.EndsWith(" stripped", StringComparison.Ordinal) };

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
    }

}
