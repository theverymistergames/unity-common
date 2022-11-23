namespace MisterGames.Tick.Jobs {

    public static class JobObserverExtensions {

        public static T ObserveBy<T>(this T job, JobObserver jobObserver) where T : IJobReadOnly {
            jobObserver.ObserveAll(job);
            return job;
        }

    }
}
