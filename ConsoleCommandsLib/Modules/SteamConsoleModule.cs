using System;
using MisterGames.Common.Steam;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.ConsoleCommandsLib.Modules {
    
    [Serializable]
    public sealed class SteamConsoleModule : IConsoleModule {
        
        public ConsoleRunner ConsoleRunner { get; set; }
        
        [ConsoleCommand("dbg/keyboard")]
        [ConsoleCommandHelp("show steam floating keyboard over console input field, works only when steam api is initialized")]
        public void ShowFloatingKeyboard() {
            if (!SteamBootstrap.IsInitialized) {
                ConsoleRunner.AppendLine("Steam API is not initialized");
                return;
            }
            
            bool shown = ConsoleRunner.ShowFloatingKeyboard();
            ConsoleRunner.AppendLine(shown 
                ? "Steam floating keyboard is shown" 
                : "Steam floating keyboard is not shown, it is available only on Steam Deck or in Big Picture mode");
        }
    }
    
}
