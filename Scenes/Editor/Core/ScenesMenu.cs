using System.Linq;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Core {

    [InitializeOnLoad]
    public static class ScenesMenu {

        static ScenesMenu() {
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
        }

        private static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange change) {
            if (change != PlayModeStateChange.ExitingEditMode) return;

            SceneLoaderSettings.SavePlaymodeStartScenes(SceneUtils.GetOpenedScenes().Select(s => s.name), SceneManager.GetActiveScene().name);
            TrySetPlaymodeStartScene(SceneLoaderSettings.Instance.rootScene.scene);   
        }
        
        private static void TrySetPlaymodeStartScene(string sceneName) {
            if (!SceneLoaderSettings.Instance.enablePlayModeStartSceneOverride) {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            var currentPlaymodeStartScene = EditorSceneManager.playModeStartScene;
            if (currentPlaymodeStartScene != null && currentPlaymodeStartScene.name == sceneName) {
                return;
            }
            
            var playModeStartScene = SceneLoaderSettings.GetAllSceneAssets().FirstOrDefault(asset => asset.name == sceneName);
            if (playModeStartScene == null) return;

            EditorSceneManager.playModeStartScene = playModeStartScene;
        }
    }
    
}
