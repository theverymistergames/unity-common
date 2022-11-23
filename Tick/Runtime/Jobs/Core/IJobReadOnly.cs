namespace MisterGames.Tick.Jobs {

    public interface IJobReadOnly {
        bool IsCompleted { get; }
    }

    public interface IJobReadOnly<out R> : IJobReadOnly {
        R Result { get; }
    }
}
