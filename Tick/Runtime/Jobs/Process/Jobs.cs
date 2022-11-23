using System;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static IJob Process(Func<float> getProcess, Action<float> action) {
            return new ProcessJob(getProcess, action);
        }

    }

}
