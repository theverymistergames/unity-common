using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.Dbg.Console.Commands {

    internal sealed class ConsoleCommandClear : IConsoleCommand {
        
        public string Name { get; } = "clear";
        public string Description { get; } = "clears console";
        
        IConsoleCommandResult IConsoleCommand.Process(DeveloperConsoleRunner runner, string[] args) {
            if (args.IsNotEmpty()) {
                return ConsoleCommandResults.Instant($"{Name} command usage: {Name}");
            } 
            
            runner.ClearConsole();
            return ConsoleCommandResults.Empty;
        }
    }
    
}