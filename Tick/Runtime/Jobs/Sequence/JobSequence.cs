using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    public readonly ref struct JobSequence {

        internal readonly PlayerLoopStage PlayerLoopStage;

        private readonly int _sequenceJobId;
        private readonly JobSystemSequence _jobSystem;

        public static JobSequence Create(PlayerLoopStage stage = PlayerLoopStage.Update) {
            return new JobSequence(stage);
        }

        private JobSequence(PlayerLoopStage playerLoopStage) {
            PlayerLoopStage = playerLoopStage;

            _jobSystem = JobSystems.Get<JobSystemSequence>(playerLoopStage);
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
