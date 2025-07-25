﻿using System;
using System.Linq;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Scenes.Core;
using MisterGames.Scenes.Utils;

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public class SceneConsoleModule : IConsoleModule {

        public ConsoleRunner ConsoleRunner { get; set; }
        
        [ConsoleCommand("scenes/load")]
        [ConsoleCommandHelp("load scene by name")]
        public void LoadSceneByName(string sceneName) {
            if (!ValidateSceneName(sceneName)) return;

            SceneLoader.LoadScene(sceneName);
            ConsoleRunner.AppendLine($"Loading scene {sceneName}");
        }

        [ConsoleCommand("scenes/loadi")]
        [ConsoleCommandHelp("load scene by index as in scenes/list output")]
        public void LoadSceneByIndex(int index) {
            if (!ValidateSceneIndex(index)) return;

            string sceneName = SceneLoaderSettings.GetAllSceneNames()[index];
            SceneLoader.LoadScene(sceneName);
            ConsoleRunner.AppendLine($"Loading scene {sceneName}");
        }

        [ConsoleCommand("scenes/unload")]
        [ConsoleCommandHelp("unload scene by name")]
        public void UnloadSceneByName(string sceneName) {
            if (!ValidateSceneName(sceneName)) return;

            SceneLoader.UnloadScene(sceneName);
            ConsoleRunner.AppendLine($"Loading scene {sceneName}");
        }

        [ConsoleCommand("scenes/unloadi")]
        [ConsoleCommandHelp("unload scene by index as in scenes/list output")]
        public void UnloadSceneByIndex(int index) {
            if (!ValidateSceneIndex(index)) return;

            string sceneName = SceneLoaderSettings.GetAllSceneNames()[index];
            SceneLoader.UnloadScene(sceneName);
            ConsoleRunner.AppendLine($"Loading scene {sceneName}");
        }

        [ConsoleCommand("scenes/list")]
        [ConsoleCommandHelp("list all scenes")]
        public void PrintAllScenes() {
            string[] sceneNames = SceneLoaderSettings.GetAllSceneNames();
            ConsoleRunner.AppendLine("Scenes:");

            for (int i = 0; i < sceneNames.Length; i++) {
                ConsoleRunner.AppendLine($" - [{i}] {sceneNames[i]}");
            }
        }
        
        [ConsoleCommand("scenes/loaded")]
        [ConsoleCommandHelp("list all loaded scenes")]
        public void PrintAllLoadedScenes() {
            string[] sceneNames = SceneUtils.GetOpenedScenes().Select(s => s.name).ToArray();
            ConsoleRunner.AppendLine("Scenes:");

            for (int i = 0; i < sceneNames.Length; i++) {
                ConsoleRunner.AppendLine($" - [{i}] {sceneNames[i]}");
            }
        }

        private bool ValidateSceneName(string sceneName) {
            string[] sceneNames = SceneLoaderSettings.GetAllSceneNames();
            if (!sceneNames.Contains(sceneName)) {
                ConsoleRunner.AppendLine($"Scene with name {sceneName} is not found. Type scenes/list to see all scenes.");
                return false;
            }

            return true;
        }

        private bool ValidateSceneIndex(int sceneIndex) {
            string[] sceneNames = SceneLoaderSettings.GetAllSceneNames();
            if (sceneIndex < 0 || sceneIndex >= sceneNames.Length) {
                ConsoleRunner.AppendLine($"Scene with index {sceneIndex} is not found. Type scenes/list to see all scenes.");
                return false;
            }

            return true;
        }
    }

}
