using MisterGames.Scenes.Editor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Actors.Editor.Builds {
    
    public sealed class ActorsBuildPreprocessor : IPreprocessBuildWithReportPerScene {
        
        public void OnPreprocessBuildForScene(BuildReport report, Scene scene) {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++) {
                PrewarmActorRoots(roots[i].transform);
            }
        }
        
        private static void PrewarmActorRoots(Transform root) {
            var actorRoots = root.GetComponentsInChildren<ActorRoot>();
            for (int i = 0; i < actorRoots.Length; i++) {
                actorRoots[i].PrewarmActorComponents(forceUpdate: true);
            }
        }
    }
    
}