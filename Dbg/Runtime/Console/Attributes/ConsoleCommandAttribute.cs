using System;

namespace MisterGames.Dbg.Console.Attributes {

    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommandAttribute : Attribute {

        public readonly string cmd;

        public ConsoleCommandAttribute(string cmd) {
            this.cmd = cmd;
        }
    }

}
