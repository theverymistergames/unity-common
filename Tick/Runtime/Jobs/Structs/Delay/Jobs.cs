using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public static partial class Jobs {

        public static Job Delay(ITimeSource timeSource, float delay) {
            return CreateJob<JobSystemDelay, float>(timeSource, delay);
        }

    }

}
