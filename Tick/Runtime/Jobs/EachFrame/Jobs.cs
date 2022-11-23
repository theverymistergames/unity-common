using System;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static IJob EachFrame(Action action) {
            return new EachFrameWhileJob(() => {
                action.Invoke();
                return true;
            });
        }

        public static IJob EachFrameWhile(Func<bool> actionWhile) {
            return new EachFrameWhileJob(actionWhile);
        }
    }

}
