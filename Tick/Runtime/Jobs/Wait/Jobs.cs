namespace MisterGames.Tick.Jobs {

    public static partial class Jobs {

        public static IJob Wait(IJobReadOnly job) {
            return new WaitAllJob(job);
        }

        public static IJob<R> Wait<R>(IJobReadOnly<R> job) {
            return new WaitJob<R>(job);
        }

        public static IJob WaitAll(params IJobReadOnly[] jobs) {
            return new WaitAllJob(jobs);
        }
    }

}
