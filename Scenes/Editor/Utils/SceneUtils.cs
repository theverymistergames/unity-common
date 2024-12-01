using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Utils {
    
    public static class SceneUtils {
        
        /// <summary>
        /// Removes .unity from SceneAsset file path.
        /// </summary>
        public static string RemoveSceneAssetFileFormat(string sceneAssetPath) {
            return sceneAssetPath[..^6];
        }

        public static IReadOnlyList<Scene> GetOpenedScenes() {
            var openedScenes = new List<Scene>();
            int sceneCount = SceneManager.sceneCount;

            for (int i = 0; i < sceneCount; i++) {
                openedScenes.Add(SceneManager.GetSceneAt(i));
            }

            return openedScenes;
        }

        public static string GetGameObjectPathInScene(Transform transform) {
            var sb = new StringBuilder(transform.name);

            while (transform.parent != null) {
                sb.Insert(0, '/');
                sb.Insert(0, transform.parent.name);
                transform = transform.parent;
            }

            return sb.ToString();
        }
    }
    
}