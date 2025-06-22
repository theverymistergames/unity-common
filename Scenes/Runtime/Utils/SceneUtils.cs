using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Scenes.Utils {
    
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

#if UNITY_EDITOR
        public static bool ShowSaveSceneDialogAndUnload_EditorOnly(Scene scene) {
            if (scene.isDirty) {
                int dialogResult = EditorUtility.DisplayDialogComplex(
                    "Scene have been modified",
                    $"Do you want to save the changes in the scene:\n{scene.path}",
                    "Save", "Cancel", "Discard"
                );

                switch (dialogResult) {
                    // Save
                    case 0:
                        EditorSceneManager.SaveScene(scene);
                        break;

                    // Cancel
                    case 1:
                        return false;

                    // Don't Save
                    case 2:
                        break;
                }	
            }
			
            SceneManager.UnloadSceneAsync(scene);
            return true;
        }  
#endif
    }
    
}