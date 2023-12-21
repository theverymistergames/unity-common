namespace MisterGames.Common.Conditions {

    public interface ICondition {
        bool IsMatched { get; }
    }

    public interface ICondition<in T> {
        bool IsMatched(T context);
    }

}
