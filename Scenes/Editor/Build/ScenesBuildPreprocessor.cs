using System;
using System.Linq;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Utils;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;

namespace MisterGames.Scenes.Editor.Build {
    
    public sealed class ScenesBuildPreprocessor : IPreprocessBuildWithReport {
        
        public int callbackOrder { get; } = 0;
        
        public void OnPreprocessBuild(BuildReport report) {
            var sceneAssets = SceneLoaderSettings.GetAllSceneAssets().ToArray();
            var sceneProcessors = TypeCache.GetTypesDerivedFrom<IPreprocessBuildWithReportPerScene>()
                .Select(t => Activator.CreateInstance(t) as IPreprocessBuildWithReportPerScene)
                .ToArray();
            
            string[] openedScenes = SceneUtils.GetOpenedScenes().Select(s => s.path).ToArray();
            
            for (int i = 0; i < sceneAssets.Length; i++) {
                var sceneAsset = sceneAssets[i];
                var scene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset), OpenSceneMode.Single);

                for (int index = 0; index < sceneProcessors.Length; index++) {
                    sceneProcessors[index]?.OnPreprocessBuildForScene(report, scene);
                }
            }
            
            for (int i = 0; i < openedScenes.Length; i++) {
                EditorSceneManager.OpenScene(openedScenes[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
            }
        }
    }
    
}