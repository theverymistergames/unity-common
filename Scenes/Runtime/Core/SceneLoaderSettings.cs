using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using System.Threading.Tasks;
using UnityEditor;
#endif

namespace MisterGames.Scenes.Core {

    public sealed class SceneLoaderSettings : Common.Data.ScriptableSingleton<SceneLoaderSettings> {

        public SceneReference rootScene;
        public bool enablePlayModeStartSceneOverride = true;
        public string[] searchScenesInFolders = { "Assets" };

        [HideInInspector]
        [SerializeField] private string[] _sceneNamesCache;

        [Serializable]
        private struct ScenesList {
            public List<string> sceneNames;
        }
        
        public static string[] GetAllSceneNames() {
#if UNITY_EDITOR
            if (SceneAssetsCache == null) UpdateScenesCache();
#endif
            
            return Instance._sceneNamesCache ?? Array.Empty<string>(); 
        }
        
#if UNITY_EDITOR
        private static Dictionary<string, SceneAsset> SceneAssetsCache;
        private const int UpdateCacheDelayMs = 300;
        private byte _updateCacheId;

        public static IEnumerable<SceneAsset> GetAllSceneAssets() {
            if (SceneAssetsCache == null) UpdateScenesCache();
            return SceneAssetsCache!.Values;
        }

        public static SceneAsset GetSceneAsset(string sceneName) {
            return GetAllSceneAssets().FirstOrDefault(a => a.name == sceneName);
        }

        public void IncludeScenesInBuildSettings() {
            EditorBuildSettings.scenes = GetAllSceneAssets()
                .OrderBy(sceneAsset => sceneAsset.name != rootScene.scene)
                .Select(sceneAsset => new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true))
                .ToArray();
        }

        public static void SavePlaymodeStartScene(string sceneName) {
            SavePlaymodeStartScenes(new[] { sceneName });
        }
        
        public static void SavePlaymodeStartScenes(IEnumerable<string> sceneNames, string activeSceneName = null) {
            var list = sceneNames.Distinct().ToList();
            
            if (activeSceneName != null) {
                if (!list.Contains(activeSceneName)) list.Add(activeSceneName);
                
                for (int i = 0; i < list.Count; i++) {
                    if (list[i] != activeSceneName) continue;

                    list[i] = list[0];
                    list[0] = activeSceneName;
                    break;
                }
            }

            PlayerPrefs.SetString(GetPlaymodeStartSceneKey(), JsonUtility.ToJson(new ScenesList { sceneNames = list }));
            PlayerPrefs.Save();
        }
        
        public static void DeletePlaymodeStartScenes() {
            PlayerPrefs.DeleteKey(GetPlaymodeStartSceneKey());
        }

        /// <summary>
        /// Desired active scene will be first in a list. 
        /// </summary>
        public static List<string> GetPlaymodeStartScenes() {
            return JsonUtility.FromJson<ScenesList>(PlayerPrefs.GetString(GetPlaymodeStartSceneKey())).sceneNames;
        }

        private static string GetPlaymodeStartSceneKey() {
            return $"{nameof(SceneLoaderSettings)}_playmodeStartScene";
        }

        private static IEnumerable<SceneAsset> CollectAllSceneAssets() {
            return AssetDatabase
                .FindAssets($"a:assets t:{nameof(SceneAsset)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(IsValidScenePath)
                .Select(AssetDatabase.LoadAssetAtPath<SceneAsset>);
        }

        private void OnValidate() {
            UpdateScenesCacheDelayed();
        }

        private async void UpdateScenesCacheDelayed() {
            byte id = ++_updateCacheId;
            await Task.Delay(UpdateCacheDelayMs);
            
            if (id != _updateCacheId) return;
            
            UpdateScenesCache();
        }
        
        private static void UpdateScenesCache() {
            SceneAssetsCache?.Clear();
            SceneAssetsCache ??= new Dictionary<string, SceneAsset>();

            var sceneAssets = CollectAllSceneAssets();
            foreach (var sceneAsset in sceneAssets) {
                SceneAssetsCache[sceneAsset.name] = sceneAsset;
            }

            Instance._sceneNamesCache = SceneAssetsCache.Keys.ToArray();
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssetIfDirty(Instance);
        }
        
        private static bool IsValidScenePath(string path) {
            if (string.IsNullOrEmpty(path)) return false;

            int pathLength = path.Length;
            string[] folders = Instance.searchScenesInFolders;

            for (int i = 0; i < folders.Length; i++) {
                string folderPath = folders[i];
                if (folderPath.Length > pathLength || folderPath != path[..folderPath.Length]) continue;

                return true;
            }

            return false;
        }

        private sealed class SceneAssetHelper : AssetPostprocessor {
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
                if (HasScenes(importedAssets) || HasScenes(deletedAssets) || HasScenes(movedAssets) || HasScenes(movedFromAssetPaths)) {
                    UpdateScenesCache();
                }
            }
            private static bool HasScenes(string[] assets) {
                for (int i = 0; i < assets?.Length; i++) {
                    if (assets[i]?.EndsWith(".unity") ?? false) return true;
                }
                return false;
            }
        }
#endif
    }
    
}
