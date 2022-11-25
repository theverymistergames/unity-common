using System;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static IJob EachFrame(Action action) {
            return new EachFrameJob(action);
        }

        public static IJob EachFrameWhile(Func<bool> actionWhile) {
            return new EachFrameWhileJob(actionWhile);
        }
    }

}
