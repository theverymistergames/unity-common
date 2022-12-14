namespace MisterGames.Tick.Jobs {

    public interface IJobSystem {
        void Initialize(IJobIdFactory jobIdFactory);
        void DeInitialize();

        bool IsJobCompleted(int jobId);
        void DisposeJob(int jobId);

        void StartJob(int jobId);
        void StopJob(int jobId);
    }

}
