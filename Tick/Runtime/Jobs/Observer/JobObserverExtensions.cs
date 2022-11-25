namespace MisterGames.Tick.Jobs {

    public static class JobObserverExtensions {

        public static T ObserveBy<T>(this T job, JobObserver jobObserver) where T : class, IJobReadOnly {
            jobObserver.Observe(job);
            return job;
        }

    }
}
