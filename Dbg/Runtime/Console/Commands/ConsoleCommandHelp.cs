using System.Collections.Generic;
using System.Linq;
using System.Text;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.Dbg.Console.Commands {
    
    internal class ConsoleCommandHelp : IConsoleCommand {
        
        public static readonly IConsoleCommandResult NoSuchCommand = 
            ConsoleCommandResults.Instant("No such command, type help to see list of all commands");
        
        public string Name => "help";
        public string Description => $"types list of all commands";
        
        private readonly Dictionary<int, string> _commands = new Dictionary<int, string>();
        private IConsoleCommandResult _result;

        public void AddCommand(IConsoleCommand command) {
            int nameHash = command.Name.ToLower().GetHashCode();
            _commands[nameHash] = $"- {command.Name}: {command.Description}";
        }

        public void Initialize() {
            AddCommand(this);
                
            var commandList = _commands.Values.ToList();
            commandList.Sort();
                
            var builder = new StringBuilder();
            for (int i = 0; i < commandList.Count; i++) {
                string command = commandList[i];
                    
                builder.AppendLine(command);
            }

            _result = ConsoleCommandResults.Instant(builder.ToString());
        }

        IConsoleCommandResult IConsoleCommand.Process(DeveloperConsoleRunner runner, string[] args) {
            if (args.IsEmpty() || args.Length > 1) return _result;

            int nameHash = args[0].GetHashCode();
            return _commands.ContainsKey(nameHash)
                ? ConsoleCommandResults.Instant(_commands[nameHash]) 
                : NoSuchCommand;
        }
        
    }
    
}