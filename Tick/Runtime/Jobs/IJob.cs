namespace MisterGames.Tick.Jobs {

    public interface IJob {
        bool IsCompleted { get; }

        void Start();
        void Stop();
    }
    
}
