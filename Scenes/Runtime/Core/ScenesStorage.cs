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

#if UNITY_EDITOR
        [SerializeField] private bool _enablePlayModeStartSceneOverride = true;
        [SerializeField] private string[] _searchScenesInFolders = {
            "Assets/Scenes",
        };
#endif

        [SerializeField] private SceneReference _sceneRoot;
        internal string SceneRoot => _sceneRoot.scene;

        [SerializeField] [ReadOnly] private string _sceneStart;
        internal string SceneStart {
            get => _sceneStart;
            set => _sceneStart = value;
        }

        [SerializeField] [HideInInspector] private string[] _sceneNames;
        public string[] SceneNames => _sceneNames;

        protected override void OnSingletonInstanceLoaded() {
#if UNITY_EDITOR
            RefreshSceneNames();
            SetActiveSceneAsStartSceneIfNotSet();
            TrySetSceneRootIfNotSet();

            EditorUtility.SetDirty(this);

            TrySetPlaymodeStartScene(_sceneRoot.scene);
#endif
        }

#if UNITY_EDITOR
        internal void RefreshSceneNames() {
            _sceneNames = GetAllSceneAssets().Select(sceneAsset => sceneAsset.name).ToArray();
        }

        internal IEnumerable<SceneAsset> GetAllSceneAssets() => AssetDatabase
            .FindAssets($"a:assets t:{nameof(SceneAsset)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(IsValidPath)
            .Select(AssetDatabase.LoadAssetAtPath<SceneAsset>)
            .Where(asset => asset != null);

        private void OnValidate() {
            TrySetPlaymodeStartScene(_sceneRoot.scene);
        }

        private void SetActiveSceneAsStartSceneIfNotSet() {
            if (!string.IsNullOrEmpty(_sceneStart)) return;
            _sceneStart = SceneManager.GetActiveScene().name;
        }

        private void TrySetSceneRootIfNotSet() {
            if (!string.IsNullOrEmpty(_sceneRoot.scene)) return;
            _sceneRoot.scene = SceneManager.GetActiveScene().name;
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
            
            var sceneRootAsset = GetAllSceneAssets().FirstOrDefault(asset => asset.name == sceneName);
            if (sceneRootAsset == null) return;

            EditorSceneManager.playModeStartScene = sceneRootAsset;
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
