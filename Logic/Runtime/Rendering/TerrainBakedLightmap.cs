using System;
using System.IO;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace MisterGames.Logic.Rendering {

    [ExecuteAlways]
    public sealed class TerrainBakedLightmap : MonoBehaviour {

        [SerializeField] private Terrain _terrain;
        [SerializeField] private Texture2D _lightmapColor;
        [SerializeField] private Texture2D _lightmapDir;
        [SerializeField] private Vector4 _lightmapScaleOffset = new(1f, 1f, 0f, 0f);
        [SerializeField] private LightmapOverride[] _overrides;

        [Serializable]
        private struct LightmapOverride {
            public SceneReference[] scenes;
            public Texture2D lightmapColor;
            public Texture2D lightmapDir;
            public Vector4 lightmapScaleOffset;
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

#if UNITY_EDITOR
            EditorSceneManager.sceneOpened += OnEditorSceneOpened;
            EditorSceneManager.sceneClosed += OnEditorSceneClosed;
#endif

            ApplyLightmap();
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

#if UNITY_EDITOR
            EditorSceneManager.sceneOpened -= OnEditorSceneOpened;
            EditorSceneManager.sceneClosed -= OnEditorSceneClosed;
#endif
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            ApplyLightmap();
        }

        private void OnSceneUnloaded(Scene scene) {
            ApplyLightmap();
        }

        [Button]
        private void ApplyLightmap() {
            var lightmapColor = _lightmapColor;
            var lightmapDir = _lightmapDir;
            var lightmapScaleOffset = _lightmapScaleOffset;

            int overrideIndex = GetActiveOverrideIndex();
            if (overrideIndex >= 0) {
                ref var lightmapOverride = ref _overrides[overrideIndex];
                lightmapColor = lightmapOverride.lightmapColor;
                lightmapDir = lightmapOverride.lightmapDir;
                lightmapScaleOffset = lightmapOverride.lightmapScaleOffset;
            }

            if (_terrain == null || lightmapColor == null) {
                Debug.LogWarning($"{nameof(TerrainBakedLightmap)}: terrain or lightmap is not set on {name}, scene {gameObject.scene.name}");
                return;
            }

            var lightmaps = LightmapSettings.lightmaps;
            int index = Array.FindIndex(lightmaps, data => data != null && data.lightmapColor == lightmapColor);

            if (index < 0) {
                index = lightmaps.Length;
                Array.Resize(ref lightmaps, index + 1);
                lightmaps[index] = new LightmapData { lightmapColor = lightmapColor, lightmapDir = lightmapDir };
                LightmapSettings.lightmaps = lightmaps;
            }

            _terrain.lightmapIndex = index;
            _terrain.lightmapScaleOffset = lightmapScaleOffset;
        }

        private int GetActiveOverrideIndex() {
            for (int i = 0; i < _overrides?.Length; i++) {
                ref var lightmapOverride = ref _overrides[i];
                if (lightmapOverride.lightmapColor != null && HasLoadedScene(lightmapOverride.scenes)) return i;
            }

            return -1;
        }

        private static bool HasLoadedScene(SceneReference[] scenes) {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                for (int j = 0; j < scenes?.Length; j++) {
                    if (scenes[j].scene == scene.name) return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private const string LightmapFolderName = "TerrainLightmap";

        private void OnEditorSceneOpened(Scene scene, OpenSceneMode mode) {
            ApplyLightmap();
        }

        private void OnEditorSceneClosed(Scene scene) {
            ApplyLightmap();
        }

        [ContextMenu("Capture Terrain Lightmap")]
        private void CaptureTerrainLightmap() {
            if (_terrain == null) _terrain = GetComponent<Terrain>();

            var lightmaps = LightmapSettings.lightmaps;
            int index = _terrain != null ? _terrain.lightmapIndex : -1;

            if (index < 0 || index >= lightmaps.Length || lightmaps[index]?.lightmapColor == null) {
                Debug.LogError($"{nameof(TerrainBakedLightmap)}: terrain {(_terrain != null ? _terrain.name : "null")} has no baked lightmap, index {index}, total lightmaps {lightmaps.Length}");
                return;
            }

            string folder = GetOrCreateLightmapFolder();

            Undo.RecordObject(this, nameof(CaptureTerrainLightmap));

            _lightmapColor = CopyLightmapTexture(lightmaps[index].lightmapColor, folder, "TerrainLightmap_light");
            _lightmapDir = CopyLightmapTexture(lightmaps[index].lightmapDir, folder, "TerrainLightmap_dir");
            _lightmapScaleOffset = _terrain.lightmapScaleOffset;

            EditorUtility.SetDirty(this);
        }

        private string GetOrCreateLightmapFolder() {
            var scene = gameObject.scene;
            string sceneDir = Path.GetDirectoryName(scene.path)?.Replace('\\', '/');
            string sceneFolder = $"{sceneDir}/{scene.name}";
            string folder = $"{sceneFolder}/{LightmapFolderName}";

            if (!AssetDatabase.IsValidFolder(sceneFolder)) AssetDatabase.CreateFolder(sceneDir, scene.name);
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(sceneFolder, LightmapFolderName);

            return folder;
        }

        private static Texture2D CopyLightmapTexture(Texture2D source, string folder, string fileName) {
            if (source == null) return null;

            string sourcePath = AssetDatabase.GetAssetPath(source);
            string targetPath = $"{folder}/{fileName}{Path.GetExtension(sourcePath)}";

            if (sourcePath == targetPath) return source;

            AssetDatabase.DeleteAsset(targetPath);
            AssetDatabase.CopyAsset(sourcePath, targetPath);

            return AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
        }
#endif
    }

}
