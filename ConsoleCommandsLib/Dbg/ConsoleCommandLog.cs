using System;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Core;
using UnityEngine;

namespace MisterGames.ConsoleCommandsLib {

    [Serializable]
    public sealed class ConsoleCommandLog : IConsoleCommand {
        
        public string Name => "log";
        public string Description => "Debug.Log";

        IConsoleCommandResult IConsoleCommand.Process(string[] args) {
            if (args.IsEmpty()) return ConsoleCommandResults.Instant($"{Name} command usage: {Name} somelog");

            string text = string.Join(" ", args);
            Debug.Log(text);
            
            return ConsoleCommandResults.Empty;
        }
    }
}
