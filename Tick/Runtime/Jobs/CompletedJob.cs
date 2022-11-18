namespace MisterGames.Tick.Jobs {

    internal sealed class CompletedJob : IJob {

        public bool IsCompleted => true;

        public void Start() { }

        public void Stop() { }
    }
}
