namespace MisterGames.Tick.Jobs {

    internal sealed class CompletedJob : IJob {

        public bool IsCompleted => true;
        public float Progress => 1f;

        public void Start() { }
        public void Stop() { }
    }
}
