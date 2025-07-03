using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Build {
    
    internal static class BuildScenesTracker {
        
        static BuildScenesTracker() {
            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
        }
        
        private static void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode) {
            UpdateBuildScenesList();
        }

        [MenuItem("MisterGames/Scenes/Update build scenes list")]
        private static void UpdateBuildScenesList() {
            EditorBuildSettings.scenes = SceneLoaderSettings.GetAllSceneAssets()
                .Select(sceneAsset => new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true))
                .ToArray();
        }
    }
    
}