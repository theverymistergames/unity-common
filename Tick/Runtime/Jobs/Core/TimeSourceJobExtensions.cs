using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public static class TimeSourceJobExtensions {

        public static void Run(this ITimeSource timeSource, IJob job) {
            job.Start();
            if (!job.IsCompleted && job is IUpdate update) timeSource.Subscribe(update);
        }

        public static T RunFrom<T>(this T job, ITimeSource timeSource) where T : IJob {
            timeSource.Run(job);
            return job;
        }

    }

}
