using System;
using System.Linq;
using MisterGames.Common.Save;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public class SaveConsoleModule : IConsoleModule {

        public ConsoleRunner ConsoleRunner { get; set; }
        
        [ConsoleCommand("saves/print")]
        [ConsoleCommandHelp("print all saves")]
        public void PrintSaves() {
            ConsoleRunner.AppendLine(string.Join("\n", SaveSystem.Instance.GetSaves().Select((s, i) => $"[{i}] :: {s}")));
        }
        
        [ConsoleCommand("saves/load")]
        [ConsoleCommandHelp("load save by id")]
        public void LoadSave(string saveId) {
            ConsoleRunner.AppendLine($"Loading save {saveId}");
            SaveSystem.Instance.TryLoad(saveId);
        }
        
        [ConsoleCommand("saves/loadi")]
        [ConsoleCommandHelp("load save by index")]
        public void LoadSaveByIndex(int index) {
            var saves = SaveSystem.Instance.GetSaves();
            if (index < 0 || index > saves.Count - 1) return;
            
            ConsoleRunner.AppendLine($"Loading save {saves[index]}");
            SaveSystem.Instance.TryLoad(saves[index].id);
        }
        
        [ConsoleCommand("saves/loadlast")]
        [ConsoleCommandHelp("load last save")]
        public void LoadLastSave() {
            string saveId = SaveSystem.Instance.GetLastWrittenSave();
            ConsoleRunner.AppendLine($"Loading save {saveId}");
            SaveSystem.Instance.TryLoad(saveId);
        }
        
        [ConsoleCommand("saves/reload")]
        [ConsoleCommandHelp("reload active save")]
        public void ReloadSave() {
            string saveId = SaveSystem.Instance.GetActiveSave();
            ConsoleRunner.AppendLine($"Loading save {saveId}");
            SaveSystem.Instance.TryLoad(saveId);
        }

        [ConsoleCommand("saves/save")]
        [ConsoleCommandHelp("save by id")]
        public void Save(string saveId) {
            ConsoleRunner.AppendLine($"Saving {saveId}");
            SaveSystem.Instance.Save(saveId);
        }
        
        [ConsoleCommand("saves/savei")]
        [ConsoleCommandHelp("save by index")]
        public void Save(int index) {
            var saves = SaveSystem.Instance.GetSaves();
            if (index < 0 || index > saves.Count - 1) return;
            
            ConsoleRunner.AppendLine($"Saving {saves[index]}");
            SaveSystem.Instance.Save(saves[index].id);
        }
        
        [ConsoleCommand("saves/delete")]
        [ConsoleCommandHelp("delete save by id")]
        public void DeleteSave(string saveId) {
            ConsoleRunner.AppendLine($"Deleting {saveId}");
            SaveSystem.Instance.DeleteSave(saveId);
        }
        
        [ConsoleCommand("saves/deletei")]
        [ConsoleCommandHelp("delete save by index")]
        public void DeleteSaveByIndex(int index) {
            var saves = SaveSystem.Instance.GetSaves();
            if (index < 0 || index > saves.Count - 1) return;
            
            ConsoleRunner.AppendLine($"Deleting {saves[index]}");
            SaveSystem.Instance.DeleteSave(saves[index].id);
        }
        
        [ConsoleCommand("saves/autosave")]
        [ConsoleCommandHelp("save by active save id")]
        public void Autosave() {
            string saveId = SaveSystem.Instance.GetActiveSave();
            ConsoleRunner.AppendLine($"Saving {saveId}");
            SaveSystem.Instance.Save(saveId);
        }
        
        [ConsoleCommand("saves/deleteall")]
        [ConsoleCommandHelp("delete all saves")]
        public void DeleteAllSaves() {
            ConsoleRunner.AppendLine($"Deleting all saves");
            SaveSystem.Instance.DeleteAllSaves();
        }
    }

}
