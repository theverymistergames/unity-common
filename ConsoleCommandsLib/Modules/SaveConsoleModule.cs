using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Save;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public class SaveConsoleModule : IConsoleModule, IConsoleSuggestionProvider {

        public ConsoleRunner ConsoleRunner { get; set; }
        
        public void CollectSuggestions(string cmd, IReadOnlyList<string> args, ICollection<string> suggestions) {
            if (args.Count != 1) return;
            if (cmd is not ("saves/load" or "saves/save" or "saves/delete")) return;

            suggestions.AddMatches(SaveSystem.Main.GetStorageFiles().Select(s => s.storageId).ToArray(), args[0]);
        }

        [ConsoleCommand("saves/print")]
        [ConsoleCommandHelp("print all saves")]
        public void PrintSaves() {
            ConsoleRunner.AppendLine(string.Join("\n", SaveSystem.Main.GetStorageFiles().Select((s, i) => $"[{i}] :: {s}")));
        }
        
        [ConsoleCommand("saves/load")]
        [ConsoleCommandHelp("load save by id")]
        public void LoadSave(string saveId) {
            ConsoleRunner.AppendLine($"Loading save {saveId}");
            SaveSystem.Main.LoadFromFile(saveId);
        }

        [ConsoleCommand("saves/save")]
        [ConsoleCommandHelp("save by id")]
        public void Save(string saveId) {
            ConsoleRunner.AppendLine($"Saving {saveId}");
            SaveSystem.Main.SaveIntoFile(saveId);
        }
        
        [ConsoleCommand("saves/delete")]
        [ConsoleCommandHelp("delete save by id")]
        public void DeleteSave(string saveId) {
            ConsoleRunner.AppendLine($"Deleting {saveId}");
            SaveSystem.Main.DeleteFile(saveId);
        }
        
        [ConsoleCommand("saves/deleteall")]
        [ConsoleCommandHelp("delete all saves")]
        public void DeleteAllSaves() {
            ConsoleRunner.AppendLine("Deleting all saves");
            SaveSystem.Main.DeleteAllFiles();
        }
    }

}
