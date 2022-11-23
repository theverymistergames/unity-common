namespace MisterGames.Tick.Jobs {

    public static class JobSequenceWaitExtensions {

        public static JobSequence Wait(this JobSequence sequence, IJobReadOnly job) {
            return sequence.Add(Jobs.Wait(job));
        }

        public static JobSequence<R> Wait<R>(this JobSequence sequence, IJobReadOnly<R> job) {
            return sequence.Add(Jobs.Wait(job));
        }

        public static JobSequence<R> Wait<R>(this JobSequence sequence, IJobReadOnly<R> job, out IJobReadOnly<R> resultJob) {
            return sequence.Add(Jobs.Wait(job), out resultJob);
        }

        public static JobSequence<R> Wait<R>(this JobSequence<R> sequence, IJobReadOnly job) {
            return sequence.Add(Jobs.Wait(job));
        }

        public static JobSequence<R> Wait<R>(this JobSequence<R> sequence, IJobReadOnly<R> job) {
            return sequence.Add(Jobs.Wait(job));
        }

        public static JobSequence<R> Wait<R>(this JobSequence<R> sequence, IJobReadOnly<R> job, out IJobReadOnly<R> resultJob) {
            return sequence.Add(Jobs.Wait(job), out resultJob);
        }

        public static JobSequence WaitAll(this JobSequence sequence, params IJobReadOnly[] jobs) {
            return sequence.Add(Jobs.WaitAll(jobs));
        }

        public static JobSequence<R> WaitAll<R>(this JobSequence<R> sequence, params IJobReadOnly[] jobs) {
            return sequence.Add(Jobs.WaitAll(jobs));
        }
    }

}
