using System.Reflection;

namespace MisterGames.Dbg.Console.Core {

    internal struct Command {

        public string cmd;
        public IConsoleModule module;
        public MethodInfo method;

    }

}
