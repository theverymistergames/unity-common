using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Editor.Menu {
    
    public static class SerializedReferenceUsageFinder {
        
        public readonly struct Usage {
            
            public readonly string Kind;
            public readonly string AssetPath;
            public readonly string ObjectPath;
            public readonly string OwnerType;
            public readonly string PropertyPath;

            public Usage(
                string kind,
                string assetPath,
                string objectPath,
                string ownerType,
                string propertyPath) {
                Kind = kind;
                AssetPath = assetPath;
                ObjectPath = objectPath;
                OwnerType = ownerType;
                PropertyPath = propertyPath;
            }

            public override string ToString() {
                return $"{Kind}: {AssetPath} | {ObjectPath} | {OwnerType}.{PropertyPath}";
            }
        }

        public sealed class UsageCandidate<TReferenced> where TReferenced : Object {
            
            public readonly TReferenced Target;
            public readonly Object Owner;
            public readonly SerializedObject SerializedObject;
            public readonly SerializedProperty ReferenceProperty;

            public readonly string Kind;
            public readonly string AssetPath;
            public readonly string ObjectPath;
            public readonly string OwnerType;
            public readonly string PropertyPath;

            public UsageCandidate(
                TReferenced target,
                Object owner,
                SerializedObject serializedObject,
                SerializedProperty referenceProperty,
                string kind,
                string assetPath,
                string objectPath,
                string ownerType) {
                Target = target;
                Owner = owner;
                SerializedObject = serializedObject;
                ReferenceProperty = referenceProperty.Copy();

                Kind = kind;
                AssetPath = assetPath;
                ObjectPath = objectPath;
                OwnerType = ownerType;
                PropertyPath = referenceProperty.propertyPath;
            }

            public SerializedProperty GetParentProperty() {
                return SerializedReferenceUsageFinder.GetParentProperty(SerializedObject, ReferenceProperty);
            }

            public SerializedProperty GetSiblingProperty(string siblingFieldName) {
                return GetParentProperty()?.FindPropertyRelative(siblingFieldName);
            }
        }

        private sealed class Options {
            
            public string[] SearchFolders = { "Assets" };
            public bool ScanPrefabsAndAssets = true;
            public bool ScanOpenedScenes = true;
            public bool ScanAllScenes = true;
            public Type[] OwnerTypeFilter = null;
            public bool IncludeHiddenAssetObjects = false;
        }
        
        private static bool _cancelSearchRequested;
        
        public static IReadOnlyList<Usage> FindUsagesInAssetsAndPrefabs<TReferenced>(
            TReferenced target,
            string[] searchFolders = null,
            Type[] ownerTypes = null,
            Func<UsageCandidate<TReferenced>, bool> predicate = null) where TReferenced : Object {
            
            var options = new Options {
                SearchFolders = searchFolders ?? new []{ "Assets" },
                
                ScanPrefabsAndAssets = true,
                ScanOpenedScenes = false,
                ScanAllScenes = false,
                
                OwnerTypeFilter = ownerTypes,
                IncludeHiddenAssetObjects = false,
            };

            return FindUsagesOf(target, predicate, options);
        }
        
        public static IReadOnlyList<Usage> FindUsagesInOpenedScenes<TReferenced>(
            TReferenced target,
            string[] searchFolders = null,
            Type[] ownerTypes = null,
            Func<UsageCandidate<TReferenced>, bool> predicate = null) where TReferenced : Object {
            
            var options = new Options {
                SearchFolders = searchFolders ?? new []{ "Assets" },
                
                ScanPrefabsAndAssets = false,
                ScanOpenedScenes = true,
                ScanAllScenes = false,
                
                OwnerTypeFilter = ownerTypes,
                IncludeHiddenAssetObjects = false,
            };

            return FindUsagesOf(target, predicate, options);
        }
        
        public static IReadOnlyList<Usage> FindUsagesInAllScenes<TReferenced>(
            TReferenced target,
            string[] searchFolders = null,
            Type[] ownerTypes = null,
            Func<UsageCandidate<TReferenced>, bool> predicate = null) where TReferenced : Object {
            
            var options = new Options {
                SearchFolders = searchFolders ?? new []{ "Assets" },
                
                ScanPrefabsAndAssets = false,
                ScanOpenedScenes = false,
                ScanAllScenes = true,
                
                OwnerTypeFilter = ownerTypes,
                IncludeHiddenAssetObjects = false,
            };

            return FindUsagesOf(target, predicate, options);
        }

        private static IReadOnlyList<Usage> FindUsagesOf<TReferenced>(
            TReferenced target,
            Func<UsageCandidate<TReferenced>, bool> predicate = null,
            Options options = null)
            where TReferenced : Object 
        {
            if (target == null) {
                return Array.Empty<Usage>();
            }

            options ??= new Options();
            _cancelSearchRequested = false;
            
            var results = new List<Usage>();
            var seen = new HashSet<string>();
            
            try {
                if (options.ScanPrefabsAndAssets)
                    ScanAllRegularAssets(target, predicate, options, results, seen);

                if (options.ScanPrefabsAndAssets)
                    ScanAllPrefabs(target, predicate, options, results, seen);

                if (options.ScanOpenedScenes)
                    ScanOpenedScenes(target, predicate, results, seen);

                if (options.ScanAllScenes)
                    ScanAllSceneAssets(target, predicate, options, results, seen);
            }
            finally {
                EditorUtility.ClearProgressBar();
                _cancelSearchRequested = false;
            }

            return results;
        }

        private static void ScanAllRegularAssets<TReferenced>(
            TReferenced target,
            Func<UsageCandidate<TReferenced>, bool> predicate,
            Options options,
            List<Usage> results,
            HashSet<string> seen)
            where TReferenced : Object {
            string[] guids = AssetDatabase.FindAssets(CreateAssetsSearchFilter(options.OwnerTypeFilter), options.SearchFolders);

            try {
                for (int i = 0; i < guids.Length; i++) {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    if (string.IsNullOrEmpty(path))
                        continue;

                    if (AssetDatabase.IsValidFolder(path))
                        continue;

                    if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                        continue;

                    ShowCancelableSearchProgress(
                        $"Scanning asset {i + 1}/{guids.Length}: {path}",
                        guids.Length == 0 ? 1f : (float) i / guids.Length);

                    Object[] assetObjects;

                    try {
                        assetObjects = AssetDatabase.LoadAllAssetsAtPath(path);
                    }
                    catch {
                        continue;
                    }

                    foreach (var assetObject in assetObjects) {
                        if (assetObject == null)
                            continue;

                        if (!options.IncludeHiddenAssetObjects &&
                            (assetObject.hideFlags & HideFlags.HideInHierarchy) != 0)
                            continue;

                        ScanSerializedOwner(
                            assetObject,
                            "Asset",
                            path,
                            assetObject.name,
                            target,
                            predicate,
                            results,
                            seen);
                    }
                    
                    if (IsSearchCancelled()) break;
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void ScanAllPrefabs<TReferenced>(
            TReferenced target,
            Func<UsageCandidate<TReferenced>, bool> predicate,
            Options options,
            List<Usage> results,
            HashSet<string> seen)
            where TReferenced : Object {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", options.SearchFolders);

            try {
                for (int i = 0; i < guids.Length; i++) {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                    ShowCancelableSearchProgress(
                        $"Scanning prefab {i + 1}/{guids.Length}: {prefabPath}",
                        guids.Length == 0 ? 1f : (float) i / guids.Length);

                    GameObject prefabRoot = null;

                    try {
                        prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

                        ScanGameObjectHierarchy(
                            prefabRoot,
                            "Prefab",
                            prefabPath,
                            target,
                            predicate,
                            results,
                            seen);
                    }
                    catch (Exception e) {
                        Debug.LogWarning($"Failed to scan prefab '{prefabPath}': {e.Message}");
                    }
                    finally {
                        if (prefabRoot != null)
                            PrefabUtility.UnloadPrefabContents(prefabRoot);
                    }
                    
                    if (IsSearchCancelled()) break;
                }
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void ScanOpenedScenes<TReferenced>(
            TReferenced target,
            Func<UsageCandidate<TReferenced>, bool> predicate,
            List<Usage> results,
            HashSet<string> seen)
            where TReferenced : Object {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);

                if (!scene.IsValid() || !scene.isLoaded)
                    continue;

                ScanScene(
                    scene,
                    "Open Scene",
                    target,
                    predicate,
                    results,
                    seen);
                
                if (IsSearchCancelled()) break;
            }
        }

        private static void ScanAllSceneAssets<TReferenced>(
            TReferenced target,
            Func<UsageCandidate<TReferenced>, bool> predicate,
            Options options,
            List<Usage> results,
            HashSet<string> seen)
            where TReferenced : Object {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                Debug.LogWarning("Scene scan cancelled because modified scenes were not saved.");
                return;
            }

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            string[] guids = AssetDatabase.FindAssets("t:Scene", options.SearchFolders);

            try {
                for (int i = 0; i < guids.Length; i++) {
                    string scenePath = AssetDatabase.GUIDToAssetPath(guids[i]);

                    ShowCancelableSearchProgress(
                        $"Scanning scene {i + 1}/{guids.Length}: {scenePath}",
                        guids.Length == 0 ? 1f : (float) i / guids.Length);

                    try {
                        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                        ScanScene(
                            scene,
                            "Scene",
                            target,
                            predicate,
                            results,
                            seen);
                    }
                    catch (Exception e) {
                        Debug.LogWarning($"Failed to scan scene '{scenePath}': {e.Message}");
                    }
                    
                    if (IsSearchCancelled()) break;
                }
            }
            finally {
                EditorUtility.ClearProgressBar();

                if (previousSetup != null && previousSetup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
        }

        private static void ScanScene<TReferenced>(
            Scene scene,
            string kind,
            TReferenced target,
            Func<UsageCandidate<TReferenced>, bool> predicate,
            List<Usage> results,
            HashSet<string> seen)
            where TReferenced : Object {
            foreach (var root in scene.GetRootGameObjects()) {
                ScanGameObjectHierarchy(
                    root,
                    kind,
                    scene.path,
                    target,
                    predicate,
                    results,
                    seen);
                
                if (IsSearchCancelled()) break;
            }
        }

        private static void ScanGameObjectHierarchy<TReferenced>(
            GameObject root,
            string kind,
            string assetPath,
            TReferenced target,
            Func<UsageCandidate<TReferenced>, bool> predicate,
            List<Usage> results,
            HashSet<string> seen)
            where TReferenced : Object {
            if (root == null)
                return;

            var transforms = root.GetComponentsInChildren<Transform>(true);

            foreach (var transform in transforms) {
                var gameObject = transform.gameObject;
                string gameObjectPath = GetGameObjectPath(transform);

                ScanSerializedOwner(
                    gameObject,
                    kind,
                    assetPath,
                    gameObjectPath,
                    target,
                    predicate,
                    results,
                    seen);

                var components = gameObject.GetComponents<Component>();

                foreach (var component in components) {
                    if (component == null)
                        continue;

                    ScanSerializedOwner(
                        component,
                        kind,
                        assetPath,
                        gameObjectPath,
                        target,
                        predicate,
                        results,
                        seen);
                    
                    if (IsSearchCancelled()) break;
                }
                
                if (IsSearchCancelled()) break;
            }
        }

        private static void ScanSerializedOwner<TReferenced>(
            Object owner,
            string kind,
            string assetPath,
            string objectPath,
            TReferenced target,
            Func<UsageCandidate<TReferenced>, bool> predicate,
            List<Usage> results,
            HashSet<string> seen)
            where TReferenced : Object {
            if (owner == null)
                return;

            Type ownerType = owner.GetType();
            
            SerializedObject serializedObject;

            try {
                serializedObject = new SerializedObject(owner);
                serializedObject.UpdateIfRequiredOrScript();
            }
            catch {
                return;
            }

            var property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.Next(enterChildren)) {
                enterChildren = true;

                if (property.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (property.objectReferenceValue != target)
                    continue;

                var candidate = new UsageCandidate<TReferenced>(
                    target,
                    owner,
                    serializedObject,
                    property,
                    kind,
                    assetPath,
                    objectPath,
                    ownerType.FullName);

                if (predicate != null && !predicate(candidate))
                    continue;

                var usage = new Usage(
                    kind,
                    assetPath,
                    objectPath,
                    ownerType.FullName,
                    property.propertyPath);

                string key = $"{kind}|{assetPath}|{objectPath}|{ownerType.FullName}|{property.propertyPath}";

                if (seen.Add(key))
                    results.Add(usage);
            }
        }
        
        private static bool IsSearchCancelled()
        {
            return _cancelSearchRequested;
        }

        private static bool ShowCancelableSearchProgress(string info, float progress)
        {
            if (_cancelSearchRequested)
                return true;

            bool cancelled = EditorUtility.DisplayCancelableProgressBar(
                "Finding serialized references",
                info,
                progress);

            if (cancelled)
            {
                _cancelSearchRequested = true;
                Debug.LogWarning("Serialized reference search cancelled. Returning partial results.");
            }

            return _cancelSearchRequested;
        }
        
        private static string CreateAssetsSearchFilter(Type[] types) {
            if (types == null || types.Length == 0) {
                return string.Empty;
            }
            
            var sb = new StringBuilder();
            for (int i = 0; i < types.Length; i++) {
                sb.Append($"t:{types[i].Name} ");
            }
            
            return sb.ToString();
        }

        public static SerializedProperty GetParentProperty(
            SerializedObject serializedObject,
            SerializedProperty property) {
            string propertyPath = property.propertyPath;
            int lastDot = propertyPath.LastIndexOf('.');

            if (lastDot < 0)
                return null;

            string parentPath = propertyPath.Substring(0, lastDot);
            return serializedObject.FindProperty(parentPath);
        }

        private static string GetGameObjectPath(Transform transform) {
            var stack = new Stack<string>();

            while (transform != null) {
                stack.Push(transform.name);
                transform = transform.parent;
            }

            return string.Join("/", stack);
        }
    }
}