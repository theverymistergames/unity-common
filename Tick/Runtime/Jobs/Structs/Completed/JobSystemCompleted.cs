namespace MisterGames.Tick.Jobs.Structs {

    internal sealed class JobSystemCompleted : IJobSystem {

        public void Initialize(IJobIdFactory jobFactory) { }

        public void DeInitialize() { }

        public bool IsJobCompleted(int jobId) {
            return true;
        }

        public int CreateJob() {
            return -1;
        }

        public void StartJob(int jobId) { }

        public void StopJob(int jobId) { }
    }

}
