using System;
using System.Collections.Generic;
using System.Reflection;
using MisterGames.Dbg.Console.Attributes;

namespace MisterGames.Dbg.Console.Core {

    internal sealed class Console {

        public IReadOnlyList<Command> Commands => _commands;
        private readonly List<Command> _commands = new List<Command>();

        public void AddModule(IConsoleModule module) {
            CollectModuleCommands(module);
        }

        public void ClearModules() {
            _commands.Clear();
        }

        public void Run(string input) {
            if (string.IsNullOrEmpty(input)) return;
            if (!TryGetCommand(input, out var command, out object[] args)) return;

            command.method.Invoke(command.module, args);
        }

        private void CollectModuleCommands(IConsoleModule module) {
            var methods = module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++) {
                var method = methods[i];
                if (!TryGetCmdFromMethod(method, out string cmd)) continue;

                var command = new Command {
                    cmd = cmd,
                    module = module,
                    method = method
                };

                _commands.Add(command);
            }
        }

        private bool TryGetCommand(string input, out Command command, out object[] args) {
            command = default;
            args = Array.Empty<object>();

            int inputLength = input.Length;

            for (int i = 0; i < _commands.Count; i++) {
                var c = _commands[i];

                string cmd = c.cmd;
                int cmdLength = cmd.Length;
                if (inputLength < cmdLength) continue;

                string possibleCommand = input[..cmdLength];
                if (cmd != possibleCommand || inputLength > cmdLength && input[cmdLength] != ' ') continue;

                string[] argStrings = GetArgs(c, input);
                int argsLength = argStrings.Length;

                var parameters = c.method.GetParameters();
                if (parameters.Length != argsLength) continue;

                args = new object[argsLength];
                for (int p = 0; p < argsLength; p++) {
                    var parameter = parameters[p];
                    if (!TryConvertArg(argStrings[p], parameter.ParameterType, out object arg))
                        return false;

                    args[p] = arg;
                }

                command = c;
                return true;
            }

            return false;
        }

        private static bool TryGetCmdFromMethod(ICustomAttributeProvider methodInfo, out string cmd) {
            cmd = string.Empty;

            object[] cmdAttrs = methodInfo.GetCustomAttributes(typeof(ConsoleCommandAttribute), false);
            if (cmdAttrs.Length == 0) return false;

            var attr = (ConsoleCommandAttribute) cmdAttrs[0];
            cmd = attr.cmd.ToLower();

            return true;
        }

        private static string[] GetArgs(Command command, string input) {
            string cmd = command.cmd;

            int cmdLength = cmd.Length;
            int inputLength = input.Length;

            if (inputLength <= cmdLength) return Array.Empty<string>();

            return input
                .Substring(cmdLength, inputLength - cmdLength)
                .Split(" ", StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool TryConvertArg(string arg, Type targetType, out object result) {
            if (targetType == typeof(bool)) {
                arg = arg.ToLower();

                bool isTrue = arg == "true" || arg == "1";

                result = isTrue;
                return isTrue || arg == "false" || arg == "0";
            }

            try {
                result = Convert.ChangeType(arg, targetType);
                return true;
            }
            catch (Exception e) {
                result = default;
                return false;
            }
        }
    }

}
