using System;
using MisterGames.Common.Attributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
#endif

namespace MisterGames.Scenes.Core {

    public sealed class ScenesStorage : MisterGames.Common.Data.ScriptableSingleton<ScenesStorage> {

        [SerializeField] private bool _enablePlayModeStartSceneOverride = true;
        [SerializeField] private string[] _searchScenesInFolders = { "Assets" };

        [SerializeField] private SceneReference _sceneRoot;
        internal string SceneRoot => _sceneRoot.scene;

        [SerializeField] [ReadOnly] private SceneReference _sceneStart;
        internal string SceneStart => _sceneStart.scene;

        [SerializeField] [HideInInspector] private string[] _sceneNames;
        public string[] SceneNames => _sceneNames;

        protected override void OnSingletonInstanceLoaded() {
#if UNITY_EDITOR
            Validate();
#endif
        }

#if UNITY_EDITOR
        private void OnValidate() {
            TrySetSceneStartIfNotSet();
            TrySetSceneRootIfNotSet();
            TrySetPlaymodeStartScene(_sceneRoot.scene);
        }

        internal void Validate() {
            RefreshSceneNames();

            TrySetSceneStartIfNotSet();
            TrySetSceneRootIfNotSet();
            TrySetPlaymodeStartScene(_sceneRoot.scene);

            EditorUtility.SetDirty(this);
        }

        internal IEnumerable<SceneAsset> GetAllSceneAssets() => AssetDatabase
            .FindAssets($"a:assets t:{nameof(SceneAsset)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(IsValidPath)
            .Select(AssetDatabase.LoadAssetAtPath<SceneAsset>)
            .Where(asset => asset != null);

        private void RefreshSceneNames() {
            _sceneNames = GetAllSceneAssets().Select(sceneAsset => sceneAsset.name).ToArray();
        }

        private void TrySetSceneStartIfNotSet() {
            if (!string.IsNullOrEmpty(_sceneStart.scene)) return;

            _sceneStart.scene = SceneManager.GetActiveScene().name;
        }

        private void TrySetSceneRootIfNotSet() {
            if (!string.IsNullOrEmpty(_sceneRoot.scene)) return;

            _sceneRoot.scene = GetAllSceneAssets()
                .FirstOrDefault(s =>
                    s.name.Contains("global", StringComparison.InvariantCultureIgnoreCase) ||
                    s.name.Contains("root", StringComparison.InvariantCultureIgnoreCase)
                )
                ?.name;
        }
        
        private void TrySetPlaymodeStartScene(string sceneName) {
            if (!_enablePlayModeStartSceneOverride) {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            var currentPlaymodeStartScene = EditorSceneManager.playModeStartScene;
            if (currentPlaymodeStartScene != null && currentPlaymodeStartScene.name == sceneName) {
                return;
            }
            
            var playModeStartScene = GetAllSceneAssets().FirstOrDefault(asset => asset.name == sceneName);
            if (playModeStartScene == null) return;

            EditorSceneManager.playModeStartScene = playModeStartScene;
        }

        private bool IsValidPath(string path) {
            if (string.IsNullOrEmpty(path)) return false;

            int pathLength = path.Length;

            for (int i = 0; i < _searchScenesInFolders.Length; i++) {
                string folderPath = _searchScenesInFolders[i];
                int folderPathLength = folderPath.Length;

                if (folderPathLength > pathLength || folderPath != path[..folderPathLength]) continue;

                return true;
            }

            return false;
        }
#endif
    }
}
