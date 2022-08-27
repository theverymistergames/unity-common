using System.Reflection;

namespace MisterGames.Dbg.Console.Core {

    public struct Command {

        public string cmd;
        public IConsoleModule module;
        public MethodInfo method;

    }

}
