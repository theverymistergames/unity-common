using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Core {

    [InitializeOnLoad]
    public static class ScenesMenu {

        static ScenesMenu() {
            string sceneName = SceneManager.GetActiveScene().name;

            if (!string.IsNullOrEmpty(sceneName)) {
                var scenesStorage = ScenesStorage.Instance;
                scenesStorage.SceneStart = sceneName;
                EditorUtility.SetDirty(scenesStorage);
            }

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            EditorSceneManager.newSceneCreated -= OnNewSceneCreated;
            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
        }

        [MenuItem("MisterGames/Tools/Include ScenesStorage scenes in build settings")]
        internal static void IncludeAllScenesInBuildSettings() {
            EditorBuildSettings.scenes = ScenesStorage.Instance.GetAllSceneAssets()
                .Select(sceneAsset => new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true))
                .ToArray();
        }

        internal static string RemoveSceneAssetFileFormat(string sceneAssetPath) {
            return sceneAssetPath.Substring(0, sceneAssetPath.Length - 6);
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
            var scenesStorage = ScenesStorage.Instance;
            scenesStorage.SceneStart = scene.name;
            EditorUtility.SetDirty(scenesStorage);
        }

        private static void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode) {
            ScenesStorage.Instance.RefreshSceneNames();
            EditorUtility.SetDirty(ScenesStorage.Instance);
        }
    }
    
}
