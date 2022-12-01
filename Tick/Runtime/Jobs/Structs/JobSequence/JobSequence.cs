using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public readonly ref struct JobSequence {

        public readonly ITimeSource timeSource;
        private readonly JobSystemSequence _jobSystemSequence;
        private readonly int _sequenceJobId;

        public static JobSequence Create(ITimeSource timeSource) {
            return new JobSequence(timeSource);
        }

        private JobSequence(ITimeSource timeSource) {
            this.timeSource = timeSource;

            _jobSystemSequence = Jobs.GetJobSystem<JobSystemSequence>(timeSource);
            _sequenceJobId = _jobSystemSequence.CreateJob();
        }

        public JobSequence Add(Job job) {
            _jobSystemSequence.AddJobIntoSequence(_sequenceJobId, job);
            return this;
        }

        public Job Start() {
            _jobSystemSequence.StartJob(_sequenceJobId);
            return new Job(_sequenceJobId, _jobSystemSequence);
        }
    }

}
