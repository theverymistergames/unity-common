using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public readonly ref struct JobSequence {

        private readonly int _sequenceJobId;
        private readonly JobSystemSequence _jobSystem;

        public static JobSequence Create(PlayerLoopStage stage = PlayerLoopStage.Update) {
            var jobSystem = JobSystems.Get<JobSystemSequence>(stage);
            return new JobSequence(jobSystem);
        }

        private JobSequence(JobSystemSequence jobSystem) {
            _jobSystem = jobSystem;
            _sequenceJobId = _jobSystem.CreateJob();
        }

        public JobSequence Add(Job job) {
            _jobSystem.AddJobIntoSequence(_sequenceJobId, job);
            return this;
        }

        public Job Push() {
            return new Job(_sequenceJobId, _jobSystem);
        }
    }

}
