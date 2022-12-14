namespace MisterGames.Tick.Jobs {

    public readonly ref struct JobObserver {

        private readonly int _observerJobId;
        private readonly JobSystemObserver _jobSystem;

        public static JobObserver Create() {
            var jobSystem = JobSystems.Get<JobSystemObserver>();
            return new JobObserver(jobSystem);
        }

        private JobObserver(JobSystemObserver jobSystem) {
            _jobSystem = jobSystem;
            _observerJobId = _jobSystem.CreateJob();
        }

        public JobObserver Observe(ReadOnlyJob job) {
            _jobSystem.Observe(_observerJobId, job);
            return this;
        }

        public ReadOnlyJob Push() {
            return new Job(_observerJobId, _jobSystem);
        }
    }

}
