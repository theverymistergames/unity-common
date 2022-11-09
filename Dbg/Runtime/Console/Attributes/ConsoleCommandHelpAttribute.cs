using System;

namespace MisterGames.Dbg.Console.Attributes {

    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommandHelpAttribute : Attribute {

        public readonly string text;

        public ConsoleCommandHelpAttribute(string text) {
            this.text = text;
        }
    }

}
