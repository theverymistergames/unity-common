using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Scenes.Core;

namespace MisterGames.ConsoleCommandsLib.Modules {

    public class SceneConsoleModule : IConsoleModule {

        [ConsoleCommand("scenes/load")]
        [ConsoleCommandHelp("load scene by name")]
        public void LoadSceneByName(string sceneName) {
            string[] sceneNames = ScenesStorage.Instance.SceneNames;
            if (!sceneNames.Contains(sceneName)) {
                ConsoleRunner.Instance.AppendLine($"Scene with name {sceneName} is not found. Type scenes/list to see all scenes.");
                return;
            }

            SceneLoader.LoadScene(sceneName);
            ConsoleRunner.Instance.AppendLine($"Loading scene {sceneName}");
        }

        [ConsoleCommand("scenes/loadi")]
        [ConsoleCommandHelp("load scene by index as in scenes/list output")]
        public void LoadSceneByIndex(int index) {
            string[] sceneNames = ScenesStorage.Instance.SceneNames;
            if (index < 0 || index >= sceneNames.Length) {
                ConsoleRunner.Instance.AppendLine($"Scene with index {index} is not found. Type scenes/list to see all scenes.");
                return;
            }

            string sceneName = sceneNames[index];
            SceneLoader.LoadScene(sceneName);
            ConsoleRunner.Instance.AppendLine($"Loading scene {sceneName}");
        }

        [ConsoleCommand("scenes/list")]
        [ConsoleCommandHelp("list all scenes")]
        public void PrintAllScenes() {
            string[] sceneNames = ScenesStorage.Instance.SceneNames;
            ConsoleRunner.Instance.AppendLine("Scenes:");

            for (int i = 0; i < sceneNames.Length; i++) {
                ConsoleRunner.Instance.AppendLine($" - [{i}] {sceneNames[i]}");
            }
        }
    }

}
