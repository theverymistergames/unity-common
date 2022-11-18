using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;

namespace MisterGames.Tick.Utils {

    public static class JobExtensions {

        public static IJob StartFrom(this IJob job, ITimeSource timeSource) {
            timeSource.Run(job);
            return job;
        }

        public static IJobReadOnly ObserveBy(this IJobReadOnly job, JobObserver jobObserver) {
            jobObserver.Observe(job);
            return job;
        }

        public static IJob ObserveBy(this IJob job, JobObserver jobObserver) {
            jobObserver.Observe(job);
            return job;
        }
    }

}
