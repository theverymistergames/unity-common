using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Core {

    [InitializeOnLoad]
    public static class ScenesMenu {

        static ScenesMenu() {
            ScenesStorage.Instance.Validate();

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

        /// <summary>
        /// Removes .unity from SceneAsset file path.
        /// </summary>
        internal static string RemoveSceneAssetFileFormat(string sceneAssetPath) {
            return sceneAssetPath[..^6];
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
            ScenesStorage.Instance.Validate();
        }

        private static void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode) {
            ScenesStorage.Instance.Validate();
        }
    }
    
}
