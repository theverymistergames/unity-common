using System;
using MisterGames.Input.Bindings;

namespace MisterGames.Dbg.Console.Attributes {

    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleHotkeyAttribute : Attribute {

        public readonly string cmd;
        public readonly KeyBinding key;
        public readonly ShortcutModifiers mod;

        public ConsoleHotkeyAttribute(
            string cmd, 
            KeyBinding key = KeyBinding.None, 
            ShortcutModifiers mod = ShortcutModifiers.None) 
        {
            this.cmd = cmd;
            this.key = key;
            this.mod = mod;
        }
    }

}
