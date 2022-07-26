using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Core;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.ConsoleCommandsLib {

    public sealed class ConsoleCommandScene : IConsoleCommand {
        
        public string Name { get; } = "scenes";
        public string Description { get; } = "load scenes, list all scenes";

        IConsoleCommandResult IConsoleCommand.Process(DeveloperConsoleRunner runner, string[] args) {
            if (args.IsEmpty()) return Usage();
            return args[0] switch {
                "load" => ExecuteLoadScene(args),
                "list" => ExecuteListScenes(args),
                _ => Usage()
            };
        }
        
        private IConsoleCommandResult Usage() => ConsoleCommandResults.Instant(
            $"{Name} command usage:\n" +
            $"- {Name} load scene_name,\n" +
            $"- {Name} load 1 (index of a scene listed by '{Name} list' command),\n" +
            $"- {Name} list"
        );

        private IConsoleCommandResult ExecuteLoadScene(string[] args) {
            if (args.Length != 2) return Usage();

            string[] sceneNames = ScenesStorage.Instance.SceneNames;
            string targetSceneName = null;
            
            if (int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int sceneIndex)) 
            {
                if (sceneIndex < 0 || sceneIndex >= sceneNames.Length) {
                    return ConsoleCommandResults.Instant(
                        $"Scene with index {sceneIndex} not found.\n" +
                        $"Existent scenes:\n{GetSceneListText(sceneNames)}"
                    );
                }

                targetSceneName = sceneNames[sceneIndex];
            }
            else {
                targetSceneName = args[1];
                    
                if (!sceneNames.Contains(targetSceneName)) {
                    return ConsoleCommandResults.Instant(
                        $"Scene '{targetSceneName}' not found.\n" +
                        $"Existent scenes:\n{GetSceneListText(sceneNames)}"
                    );
                }
            }
            
            var sceneLoadTask = SceneLoader.LoadScene(targetSceneName);

            return ConsoleCommandResults.Continuous(() => {
                int percents = Mathf.CeilToInt(sceneLoadTask.Progress * 100f);
                    
                string output = $"Loading scene {targetSceneName}\nProgress . . . . {percents}%";
                bool isCompleted = sceneLoadTask.IsDone;
                    
                if (isCompleted) {
                    output = $"{output}\nDone";
                }
                    
                return ConsoleCommandResults.Instant(output, isCompleted);
            });
        }

        private IConsoleCommandResult ExecuteListScenes(string[] args) {
            return args.Length == 1 
                ? ConsoleCommandResults.Instant($"{GetSceneListText(ScenesStorage.Instance.SceneNames)}") 
                : Usage();
        }
        
        private static string GetSceneListText(IReadOnlyList<string> sceneNames) {
            var builder = new StringBuilder();
            for (int i = 0; i < sceneNames.Count; i++) {
                string item = $"[{i}] {sceneNames[i]}";
                builder.AppendLine(item);
            }
            return builder.ToString();
        }
    }
}