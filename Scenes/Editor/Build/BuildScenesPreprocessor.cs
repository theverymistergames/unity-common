using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MisterGames.Scenes.Editor.Build {
    
    public sealed class BuildScenesPreprocessor : IPreprocessBuildWithReport {

        public int callbackOrder => 0;
        
        public void OnPreprocessBuild(BuildReport report) {
            var scenes = IsDevelopmentBuild(report) 
                ? SceneLoaderSettings.GetAllSceneAssets()
                : SceneLoaderSettings.GetProductionBuildSceneAssets();
            
            EditorBuildSettings.scenes = scenes
                .Select(sceneAsset => new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true))
                .ToArray();
        }

        private static bool IsDevelopmentBuild(BuildReport report) {
            return (report.summary.options & BuildOptions.Development) == BuildOptions.Development;
        }
    }
    
}