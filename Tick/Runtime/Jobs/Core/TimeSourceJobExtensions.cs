using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public static class TimeSourceJobExtensions {

        public static void Run(this ITimeSource timeSource, IJob job) {
            if (job.IsCompleted) return;

            job.Start();
            if (job.IsCompleted) return;

            if (job is IUpdate update) timeSource.Subscribe(update);
        }

        public static T RunFrom<T>(this T job, ITimeSource timeSource) where T : class, IJob {
            timeSource.Run(job);
            return job;
        }

    }

}
