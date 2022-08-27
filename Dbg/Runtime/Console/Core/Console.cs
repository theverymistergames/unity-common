using System;
using System.Collections.Generic;
using System.Reflection;
using MisterGames.Common.Lists;
using MisterGames.Dbg.Console.Attributes;

namespace MisterGames.Dbg.Console.Core {

    public sealed class Console {

        public Command[] Commands => _commands.ToArray();
        private readonly List<Command> _commands = new List<Command>();

        internal void Initialize() {
            CollectCommandsInConsoleModules();
        }

        internal void DeInitialize() {
            _commands.Clear();
        }

        internal void Run(string input) {
            if (string.IsNullOrEmpty(input)) return;
            if (!TryGetCommand(input, out var command, out object[] args)) return;

            command.method.Invoke(command.module, args);
        }

        private void CollectCommandsInConsoleModules() {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++) {
                var assembly = assemblies[a];
                var types = assembly.GetTypes();

                for (int t = 0; t < types.Length; t++) {
                    var type = types[t];
                    if (type.IsAbstract || !type.GetInterfaces().Contains(typeof(IConsoleModule))) continue;

                    var module = (IConsoleModule) Activator.CreateInstance(type);
                    CollectModuleCommands(module);
                }
            }
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

                string matched = input[..cmdLength];
                if (cmd != matched) continue;

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
