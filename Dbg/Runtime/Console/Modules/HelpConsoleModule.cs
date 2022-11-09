using System;
using System.Linq;
using System.Reflection;
using System.Text;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.Dbg.Console.Modules {

    [Serializable]
    public class HelpConsoleModule : IConsoleModule {

        public ConsoleRunner ConsoleRunner { get; set; }

        [ConsoleCommand("help")]
        [ConsoleCommandHelp("prints all commands help")]
        public void PrintAllCommandsHelp() {
            var commands = ConsoleRunner.Commands.OrderBy(c => c.cmd).ToArray();

            ConsoleRunner.AppendLine("Commands:");
            for (int i = 0; i < commands.Length; i++) {
                string help = GetHelpText(commands[i]);
                ConsoleRunner.AppendLine($" - {help}");
            }
        }

        [ConsoleCommand("help")]
        [ConsoleCommandHelp("prints command help")]
        public void PrintCommandHelp(string cmd) {
            var commands = ConsoleRunner.Commands;
            cmd = cmd.ToLower();

            bool foundAtLeastOneCommand = false;

            for (int i = 0; i < commands.Count; i++) {
                var command = commands[i];
                if (command.cmd != cmd) continue;

                string help = GetHelpText(commands[i]);
                ConsoleRunner.AppendLine($" - {help}");
                foundAtLeastOneCommand = true;
            }

            if (!foundAtLeastOneCommand) {
                ConsoleRunner.AppendLine($"Command {cmd} is not found. Type help to see list of all commands.");
            }
        }

        private static string GetHelpText(Command command) {
            var sb = new StringBuilder(command.cmd);

            string parametersText = GetMethodParametersAsString(command.method);
            if (!string.IsNullOrEmpty(parametersText)) {
                sb.Append($" {parametersText}");
            }

            string helpAttrText = GetCmdHelpFromMethod(command.method);
            if (!string.IsNullOrEmpty(helpAttrText)) {
                sb.Append($" : {helpAttrText}");
            }

            return sb.ToString();
        }

        private static string GetCmdHelpFromMethod(ICustomAttributeProvider methodInfo) {
            object[] cmdAttrs = methodInfo.GetCustomAttributes(typeof(ConsoleCommandHelpAttribute), false);
            if (cmdAttrs.Length == 0) return null;

            var attr = (ConsoleCommandHelpAttribute) cmdAttrs[0];
            return attr.text.Trim();
        }

        private static string GetMethodParametersAsString(MethodBase methodInfo) {
            var sb = new StringBuilder();

            var parameters = methodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; i++) {
                var parameter = parameters[i];
                sb.Append($"[{parameter.ParameterType.Name}] ");
            }

            return sb.ToString().Trim();
        }
    }
}
