using System;

namespace MisterGames.Dbg.Console2.Attributes {

    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommandAttribute : Attribute {

        public string cmd;

        public ConsoleCommandAttribute(string cmd) {
            this.cmd = cmd;
        }
    }

}
