using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;

namespace MisterGames.Tick.Utils {

    public static class JobExtensions {

        public static IJob StartFrom(this IJob job, ITimeSource timeSource) {
            timeSource.Run(job);
            return job;
        }

    }

}
