namespace MisterGames.Tick.Jobs {

    public interface IJob : IJobReadOnly {
        void Start();
        void Stop();
    }

    public interface IJob<out R> : IJob, IJobReadOnly<R> { }

}
