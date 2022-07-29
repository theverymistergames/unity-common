using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Commands;
using UnityEngine;

namespace MisterGames.Dbg.Console.Core {
    
    public sealed class DeveloperConsole {
        
        private readonly Dictionary<int, IConsoleCommand> _commands = new Dictionary<int, IConsoleCommand>();

        public DeveloperConsole(IReadOnlyList<IConsoleCommand> commands) {
            var helpConsoleCommand = new ConsoleCommandHelp();
            
            for (int i = 0; i < commands.Count; i++) {
                var command = commands[i];
                if (TryAddCommand(command)) helpConsoleCommand.AddCommand(command);
            }
            
            helpConsoleCommand.Initialize();
            TryAddCommand(helpConsoleCommand);
        }

        private bool TryAddCommand(IConsoleCommand command) {
            string name = command.Name.ToLower();

            if (name.IsEmpty()) {
                Debug.LogError($"{nameof(DeveloperConsole)}: command name can not be empty, skip");
                return false;
            }

            int nameHash = name.GetHashCode();

            if (_commands.ContainsKey(nameHash)) {
                Debug.LogError($"{nameof(DeveloperConsole)}: already contains command with name {command.Name}, skip");
                return false;
            }

            _commands[nameHash] = command;
            return true;
        }

        public IConsoleCommandResult ProcessCommand(string input) {
            string[] symbols = input
                .Split(' ')
                .Select(symbol => symbol.Trim())
                .Where(symbol => symbol.IsNotEmpty())
                .ToArray();

            if (symbols.IsEmpty()) return ConsoleCommandResults.Empty;
            
            string name = symbols[0];
            if (name.IsEmpty()) return ConsoleCommandHelp.NoSuchCommand;
            
            int nameHash = name.ToLower().GetHashCode();
            if (!_commands.ContainsKey(nameHash)) return ConsoleCommandHelp.NoSuchCommand;

            string[] args = symbols.Skip(1).ToArray();
            return _commands[nameHash].Process(args);
        }
    }
}
