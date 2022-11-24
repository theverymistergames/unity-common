using System;

namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static IJob EachFrameProcess(Func<float> getProcess, Action<float> action) {
            return new EachFrameProcessJob(getProcess, action);
        }

    }

}
