namespace MisterGames.Tick.Jobs {

    public interface IJob : IJobReadOnly {
        void Start();
        void Stop();

        void Reset();
    }

    public interface IJobReadOnly {
        bool IsCompleted { get; }
    }
}
