﻿using System.Linq;
using MisterGames.Scenes.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MisterGames.Scenes.Editor.Core {

    [InitializeOnLoad]
    public static class ScenesMenu {

        static ScenesMenu() {
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChangedInEditMode;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChangedInEditMode;
            
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            if (SceneManager.sceneCount > 0) SetupStartScene(SceneManager.GetSceneAt(0).name);
        }
        
        private static void OnActiveSceneChangedInEditMode(Scene arg0, Scene arg1) {
            SetupStartScene(arg1.name);
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
            SetupStartScene(scene.name);
        }

        private static void SetupStartScene(string sceneName) {
            SceneLoaderSettings.SavePlaymodeStartScene(sceneName);
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
