using System;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.Dbg.Console.Commands {

    [Serializable]
    public sealed class ConsoleCommandClear : IConsoleCommand {
        
        public string Name => "clear";
        public string Description => "clears console";

        IConsoleCommandResult IConsoleCommand.Process(string[] args) {
            if (args.IsNotEmpty()) {
                return ConsoleCommandResults.Instant($"{Name} command usage: {Name}");
            } 
            
            DeveloperConsoleRunner.Instance.ClearConsole();
            return ConsoleCommandResults.Empty;
        }
    }
    
}
