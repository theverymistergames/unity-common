using System;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static IJob Action(Action action) {
            return new ActionJob(action);
        }

        public static IJob<R> Action<R>(Func<R> func) {
            return new ActionJob<R>(func);
        }
    }

}
