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

    public sealed class SceneStorage : MisterGames.Common.Data.ScriptableSingleton<SceneStorage> {

        [SerializeField] private bool _enablePlayModeStartSceneOverride = true;
        [SerializeField] private string[] _searchScenesInFolders = { "Assets" };

        [SerializeField] private SceneReference _rootScene;
        internal string RootScene => _rootScene.scene;

        [SerializeField] [ReadOnly] private SceneReference _editorStartScene;
        internal string EditorStartScene => _editorStartScene.scene;

        [SerializeField] [HideInInspector] private string[] _sceneNames;
        public string[] SceneNames => _sceneNames;

        protected override void OnSingletonInstanceLoaded() {
#if UNITY_EDITOR
            Validate();
#endif
        }

#if UNITY_EDITOR
        private void OnValidate() {
            SetupSceneStart();
            TrySetupSceneRootIfNotSet();
            TrySetPlaymodeStartScene(_rootScene.scene);
        }

        internal void Validate() {
            RefreshSceneNames();

            TrySetupSceneRootIfNotSet();
            TrySetPlaymodeStartScene(_rootScene.scene);
            SetupSceneStart();

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

        private void SetupSceneStart() {
            string activeScene = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(activeScene)) return;

            _editorStartScene.scene = activeScene;
        }

        private void TrySetupSceneRootIfNotSet() {
            if (!string.IsNullOrEmpty(_rootScene.scene)) return;

            _rootScene.scene = GetAllSceneAssets()
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
