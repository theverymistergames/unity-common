using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public static class JobExtensions {

        public static void Run(this ITimeSource timeSource, IJob job) {
            job.Start();
            if (!job.IsCompleted && job is IUpdate update) timeSource.Subscribe(update);
        }

        public static IJob RunFrom(this IJob job, ITimeSource timeSource) {
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
