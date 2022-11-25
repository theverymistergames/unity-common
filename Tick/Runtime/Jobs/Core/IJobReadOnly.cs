namespace MisterGames.Tick.Jobs {

    public interface IJobReadOnly {
        bool IsCompleted { get; }
        float Progress { get; }
    }

    public interface IJobReadOnly<out R> : IJobReadOnly {
        R Result { get; }
    }
}
