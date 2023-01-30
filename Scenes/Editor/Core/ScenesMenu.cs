using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Core {

    [InitializeOnLoad]
    public static class ScenesMenu {

        [MenuItem("MisterGames/Tools/Include ScenesStorage scenes in build settings")]
        internal static void IncludeAllScenesInBuildSettings() {
            EditorBuildSettings.scenes = ScenesStorage.Instance.GetAllSceneAssets()
                .Select(sceneAsset => new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true))
                .ToArray();
        }


        internal static string RemoveSceneAssetFileFormat(string sceneAssetPath) {
            return sceneAssetPath.Substring(0, sceneAssetPath.Length - 6);
        }

        static ScenesMenu() {
            ScenesStorage.Instance.SceneStart = SceneManager.GetActiveScene().name;

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            EditorSceneManager.newSceneCreated -= OnNewSceneCreated;
            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
            ScenesStorage.Instance.SceneStart = scene.name;
            EditorUtility.SetDirty(ScenesStorage.Instance);
        }

        private static void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode) {
            ScenesStorage.Instance.RefreshSceneNames();
            EditorUtility.SetDirty(ScenesStorage.Instance);
        }
    }
    
}
