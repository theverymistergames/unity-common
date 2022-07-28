using System;
using System.Globalization;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.Dbg.Console.Commands {

    [Serializable]
    public sealed class ConsoleCommandSetFontSize : IConsoleCommand {
        
        public string Name { get; } = "setfontsize";
        public string Description { get; } = "set font size for console text";
        
        IConsoleCommandResult IConsoleCommand.Process(DeveloperConsoleRunner runner, string[] args) {
            if (args.Length != 1 ||
                !float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float size)) 
            {
                return ConsoleCommandResults.Instant($"{Name} command usage: {Name} 14");
            }
            
            runner.SetTextFieldFontSize(size);
            return ConsoleCommandResults.Empty;
        }
    }
    
}
