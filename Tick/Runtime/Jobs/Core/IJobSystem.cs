namespace MisterGames.Tick.Jobs {

    public interface IJobSystemReadOnly {
        void Initialize(IJobIdFactory jobIdFactory);
        void DeInitialize();

        bool IsJobCompleted(int jobId);
        void DisposeJob(int jobId);
    }

    public interface IJobSystem : IJobSystemReadOnly {
        void StartJob(int jobId);
        void StopJob(int jobId);
    }

}
