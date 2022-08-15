using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Dbg.Console2.Attributes;

namespace MisterGames.Dbg.Console2.Core {

    public sealed class Console {

        private readonly List<Command> _commands = new List<Command>();

        private struct Command {
            public string cmd;
            public IConsoleModule module;
            public MethodInfo method;
        }

        public void Initialize() {
            DeInitialize();
            CollectCommandsInConsoleModules();
        }

        public void DeInitialize() {
            _commands.Clear();
        }

        public void Run(string input) {
            if (!IsValidInput(input)) return;
            if (!TryGetCommand(input, out var command, out object[] args)) return;

            command.method.Invoke(command.module, args);
        }

        private void CollectCommandsInConsoleModules() {
            var modules = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type.GetInterfaces().Contains(typeof(IConsoleModule)))
                .Select(type => (IConsoleModule) Activator.CreateInstance(type));

            foreach (var module in modules) {
                CollectModuleCommands(module);
            }
        }

        private void CollectModuleCommands(IConsoleModule module) {
            var methods = module
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(IsValidMethod);

            foreach (var method in methods) {
                if (!TryGetCommandCmd(method, out string cmd)) continue;

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

        private static bool TryGetCommandCmd(ICustomAttributeProvider methodInfo, out string cmd) {
            cmd = string.Empty;

            object[] fastLinkAttrs = methodInfo.GetCustomAttributes(typeof(ConsoleCommandAttribute), false);
            if (fastLinkAttrs.Length == 0) return false;

            var attr = (ConsoleCommandAttribute) fastLinkAttrs[0];
            cmd = attr.cmd.ToLower();

            return true;
        }

        private static string[] GetArgs(Command fastLink, string input) {
            string cmd = fastLink.cmd;

            int cmdLength = cmd.Length;
            int inputLength = input.Length;

            if (inputLength <= cmdLength) return Array.Empty<string>();

            return input
                .Substring(cmdLength, inputLength - cmdLength)
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .ToArray();
        }

        private static bool TryConvertArg(string arg, Type targetType, out object result) {
            if (targetType == typeof(bool)) {
                result = arg is "true" or "1";
                return arg is "true" or "false" or "1" or "0";
            }

            try {
                result = Convert.ChangeType(arg, targetType);
                return true;
            }
            catch (Exception e) {
                result = null;
                return false;
            }
        }

        private static bool IsValidInput(string input) {
            return !string.IsNullOrEmpty(input);
        }

        private static bool IsValidMethod(MethodInfo methodInfo) {
            string name = methodInfo.Name;
            return name is not ("GetHashCode" or "ToString" or "Equals" or "GetType");
        }
    }

}
