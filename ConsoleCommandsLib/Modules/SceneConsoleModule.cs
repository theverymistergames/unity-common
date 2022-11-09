using System;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Scenes.Core;

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public class SceneConsoleModule : IConsoleModule {

        public ConsoleRunner ConsoleRunner { get; set; }
        
        [ConsoleCommand("scenes/load")]
        [ConsoleCommandHelp("load scene by name")]
        public void LoadSceneByName(string sceneName) {
            string[] sceneNames = ScenesStorage.Instance.SceneNames;
            if (!sceneNames.Contains(sceneName)) {
                ConsoleRunner.AppendLine($"Scene with name {sceneName} is not found. Type scenes/list to see all scenes.");
                return;
            }

            SceneLoader.LoadScene(sceneName);
            ConsoleRunner.AppendLine($"Loading scene {sceneName}");
        }

        [ConsoleCommand("scenes/loadi")]
        [ConsoleCommandHelp("load scene by index as in scenes/list output")]
        public void LoadSceneByIndex(int index) {
            string[] sceneNames = ScenesStorage.Instance.SceneNames;
            if (index < 0 || index >= sceneNames.Length) {
                ConsoleRunner.AppendLine($"Scene with index {index} is not found. Type scenes/list to see all scenes.");
                return;
            }

            string sceneName = sceneNames[index];
            SceneLoader.LoadScene(sceneName);
            ConsoleRunner.AppendLine($"Loading scene {sceneName}");
        }

        [ConsoleCommand("scenes/list")]
        [ConsoleCommandHelp("list all scenes")]
        public void PrintAllScenes() {
            string[] sceneNames = ScenesStorage.Instance.SceneNames;
            ConsoleRunner.AppendLine("Scenes:");

            for (int i = 0; i < sceneNames.Length; i++) {
                ConsoleRunner.AppendLine($" - [{i}] {sceneNames[i]}");
            }
        }
    }

}
