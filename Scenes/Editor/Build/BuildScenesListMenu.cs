using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;

namespace MisterGames.Scenes.Editor.Build {
    
    internal static class BuildScenesListMenu {

        [MenuItem("MisterGames/Scenes/Update build scenes list")]
        private static void UpdateBuildScenesList() {
            string rootScene = SceneLoaderSettings.Instance.rootScene.scene;
            EditorBuildSettings.globalScenes = SceneLoaderSettings.GetAllSceneAssets()
                .OrderBy(s => s.name == rootScene ? 0 : 1)
                .Select(sceneAsset => new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true))
                .ToArray();
        }
    }
    
}