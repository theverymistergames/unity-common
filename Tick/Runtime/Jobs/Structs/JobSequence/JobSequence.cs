using System.Buffers;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs.Structs {

    public ref struct JobSequence {

        private const int INITIAL_JOB_ARRAY_LENGTH = 10;
        private const int MAX_JOBS_IN_SEQUENCE = 20;
        private const int MAX_JOB_ARRAYS_PER_BUCKET = 100;

        private static readonly ArrayPool<Job> JobArrayPool = ArrayPool<Job>
            .Create(MAX_JOBS_IN_SEQUENCE, MAX_JOB_ARRAYS_PER_BUCKET);

        public readonly ITimeSource timeSource;

        private readonly Job[] _jobs;
        private int _jobsCount;

        public static JobSequence Create(ITimeSource timeSource) {
            return new JobSequence(timeSource);
        }

        private JobSequence(ITimeSource timeSource) {
            this.timeSource = timeSource;

            _jobs = JobArrayPool.Rent(INITIAL_JOB_ARRAY_LENGTH);
            _jobsCount = 0;
        }

        public JobSequence Add(Job job) {
            _jobs[_jobsCount++] = job;
            return this;
        }

        public Job Start() {
            return Jobs.Completed;
        }
    }

}
