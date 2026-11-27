using System;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public sealed class InputConsoleModule : IConsoleModule {

        public ConsoleRunner ConsoleRunner { get; set; }

        [ConsoleCommand("inputs/gamepad_default")]
        [ConsoleCommandHelp("any gamepad is treated as Default gamepad type, keyboard and mouse are not affected: on/off, 1/0 or true/false")]
        public void ForceDefaultGamepadType(string state) {
            if (!TryParseState(state, out bool force)) return;

            if (!Services.TryGet(out IDeviceService deviceService)) {
                ConsoleRunner.AppendLine("DeviceService is not registered");
                return;
            }

            deviceService.ForceDefaultGamepadType(force);

            ConsoleRunner.AppendLine($"Default gamepad type forced: {(force ? "on" : "off")}, current device {deviceService.CurrentDevice}");
        }

        private bool TryParseState(string state, out bool enabled) {
            string value = state?.Trim().ToLower();
            enabled = value is "on" or "1" or "true";

            if (enabled || value is "off" or "0" or "false") return true;

            ConsoleRunner.AppendLine($"Invalid state [{state}], use on/off, 1/0 or true/false");
            return false;
        }
    }

}
