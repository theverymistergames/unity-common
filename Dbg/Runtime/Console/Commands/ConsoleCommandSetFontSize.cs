using System;
using System.Globalization;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.Dbg.Console.Commands {

    [Serializable]
    public sealed class ConsoleCommandSetFontSize : IConsoleCommand {
        
        public string Name => "setfontsize";
        public string Description => "set font size for console text";

        IConsoleCommandResult IConsoleCommand.Process(string[] args) {
            if (args.Length != 1 ||
                !float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float size)) 
            {
                return ConsoleCommandResults.Instant($"{Name} command usage: {Name} 14");
            }
            
            DeveloperConsoleRunner.Instance.SetTextFieldFontSize(size);
            return ConsoleCommandResults.Empty;
        }
    }
    
}
