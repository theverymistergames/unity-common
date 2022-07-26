using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Core;
using UnityEngine;

namespace MisterGames.ConsoleCommandsLib {

    public sealed class ConsoleCommandLog : IConsoleCommand {
        
        public string Name { get; } = "log";
        public string Description { get; } = "Debug.Log";

        IConsoleCommandResult IConsoleCommand.Process(DeveloperConsoleRunner runner, string[] args) {
            if (args.IsEmpty()) return ConsoleCommandResults.Instant($"{Name} command usage: {Name} somelog");

            string text = string.Join(" ", args);
            Debug.Log(text);
            
            return ConsoleCommandResults.Empty;
        }
    }
}