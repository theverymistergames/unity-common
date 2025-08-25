using System;
using System.Linq;
using MisterGames.Common.Save;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Input.Bindings;

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public class SaveConsoleModule : IConsoleModule {

        public ConsoleRunner ConsoleRunner { get; set; }
        
        [ConsoleCommand("saves/print")]
        [ConsoleCommandHelp("print all saves")]
        public void PrintSaves() {
            ConsoleRunner.AppendLine(string.Join("\n", SaveSystem.Main.GetSaves().Select((s, i) => $"[{i}] :: {s}")));
        }
        
        [ConsoleCommand("saves/load")]
        [ConsoleCommandHelp("load save by id")]
        public void LoadSave(string saveId) {
            ConsoleRunner.AppendLine($"Loading save {saveId}");
            SaveSystem.Main.TryLoad(saveId);
        }
        
        [ConsoleCommand("saves/loadi")]
        [ConsoleCommandHelp("load save by index")]
        public void LoadSaveByIndex(int index) {
            var saves = SaveSystem.Main.GetSaves();
            if (index < 0 || index > saves.Count - 1) return;
            
            ConsoleRunner.AppendLine($"Loading save {saves[index]}");
            SaveSystem.Main.TryLoad(saves[index].id);
        }
        
        [ConsoleCommand("saves/loadlast")]
        [ConsoleCommandHelp("load last save")]
        public void LoadLastSave() {
            string saveId = SaveSystem.Main.GetLastWrittenSave();
            ConsoleRunner.AppendLine($"Loading save {saveId}");
            SaveSystem.Main.TryLoad(saveId);
        }
        
        [ConsoleCommand("saves/reload")]
        [ConsoleCommandHelp("reload active save")]
        public void ReloadSave() {
            string saveId = SaveSystem.Main.GetActiveSave();
            ConsoleRunner.AppendLine($"Loading save {saveId}");
            SaveSystem.Main.TryLoad(saveId);
        }

        [ConsoleCommand("saves/save")]
        [ConsoleCommandHelp("save by id")]
        public void Save(string saveId) {
            ConsoleRunner.AppendLine($"Saving {saveId}");
            SaveSystem.Main.Save(saveId);
        }
        
        [ConsoleCommand("saves/savei")]
        [ConsoleCommandHelp("save by index")]
        public void Save(int index) {
            var saves = SaveSystem.Main.GetSaves();
            if (index < 0 || index > saves.Count - 1) return;
            
            ConsoleRunner.AppendLine($"Saving {saves[index]}");
            SaveSystem.Main.Save(saves[index].id);
        }
        
        [ConsoleCommand("saves/delete")]
        [ConsoleCommandHelp("delete save by id")]
        public void DeleteSave(string saveId) {
            ConsoleRunner.AppendLine($"Deleting {saveId}");
            SaveSystem.Main.DeleteSave(saveId);
        }
        
        [ConsoleCommand("saves/deletei")]
        [ConsoleCommandHelp("delete save by index")]
        public void DeleteSaveByIndex(int index) {
            var saves = SaveSystem.Main.GetSaves();
            if (index < 0 || index > saves.Count - 1) return;
            
            ConsoleRunner.AppendLine($"Deleting {saves[index]}");
            SaveSystem.Main.DeleteSave(saves[index].id);
        }
        
        [ConsoleCommand("saves/autosave")]
        [ConsoleCommandHelp("save by active save id")]
        public void Autosave() {
            string saveId = SaveSystem.Main.GetActiveSave();
            ConsoleRunner.AppendLine($"Saving {saveId}");
            SaveSystem.Main.Save(saveId);
        }
        
        [ConsoleCommand("saves/deleteall")]
        [ConsoleCommandHelp("delete all saves")]
        public void DeleteAllSaves() {
            ConsoleRunner.AppendLine($"Deleting all saves");
            SaveSystem.Main.DeleteAllSaves();
        }

        [ConsoleHotkey("saves/load default_0", KeyBinding.Digit0, ShortcutModifiers.Control | ShortcutModifiers.Shift)] 
        public void LoadSave0() {}
        
        [ConsoleHotkey("saves/load default_1", KeyBinding.Digit1, ShortcutModifiers.Control | ShortcutModifiers.Shift)] 
        public void LoadSave1() {}
        
        [ConsoleHotkey("saves/load default_2", KeyBinding.Digit2, ShortcutModifiers.Control | ShortcutModifiers.Shift)] 
        public void LoadSave2() {}
        
        [ConsoleHotkey("saves/load default_3", KeyBinding.Digit3, ShortcutModifiers.Control | ShortcutModifiers.Shift)] 
        public void LoadSave3() {}
        
        [ConsoleHotkey("saves/load default_4", KeyBinding.Digit4, ShortcutModifiers.Control | ShortcutModifiers.Shift)] 
        public void LoadSave4() {}
        
        [ConsoleHotkey("saves/load default_5", KeyBinding.Digit5, ShortcutModifiers.Control | ShortcutModifiers.Shift)] 
        public void LoadSave5() {}
        
        [ConsoleHotkey("saves/load default_6", KeyBinding.Digit6, ShortcutModifiers.Control | ShortcutModifiers.Shift)] 
        public void LoadSave6() {}
        
        [ConsoleHotkey("saves/load default_7", KeyBinding.Digit7, ShortcutModifiers.Control | ShortcutModifiers.Shift)] 
        public void LoadSave7() {}
        
        [ConsoleHotkey("saves/load default_8", KeyBinding.Digit8, ShortcutModifiers.Control | ShortcutModifiers.Shift)] 
        public void LoadSave8() {}
        
        [ConsoleHotkey("saves/load default_9", KeyBinding.Digit9, ShortcutModifiers.Control | ShortcutModifiers.Shift)] 
        public void LoadSave9() {}
    }

}
