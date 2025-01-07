using System;
using MisterGames.Common.Lists;
using MisterGames.Common.Pooling;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Scenes.Core;

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public sealed class PoolConsoleModule : IConsoleModule {

        public ConsoleRunner ConsoleRunner { get; set; }
        
        [ConsoleCommand("pool/show logs")]
        [ConsoleCommandHelp("enable/disable PrefabPool logs")]
        public void ShowPoolLogs(int show) {
            if (PrefabPool.Main is not PrefabPool pool) return;

            pool.showDebugLogs = show > 0;
        }
    }

}
