namespace MisterGames.Tick.Jobs {

    public interface IJob : IJobReadOnly {
        void Start();
        void Stop();
    }

    public interface IJobReadOnly {
        bool IsCompleted { get; }
    }
}
