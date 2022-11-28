namespace MisterGames.Tick.Jobs.Structs {

    public interface IJobSystemBase {
        void Initialize(IJobIdFactory jobFactory);
        void DeInitialize();

        bool IsJobCompleted(int jobId);
        void StartJob(int jobId);
        void StopJob(int jobId);
    }

    public interface IJobSystem : IJobSystemBase {
        int CreateJob();
    }

    public interface IJobSystem<in T> : IJobSystemBase {
        int CreateJob(T data);
    }

}
