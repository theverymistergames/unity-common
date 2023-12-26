namespace MisterGames.Common.Conditions {

    public interface ICondition<in TContext> {
        bool IsMatch(TContext context);
    }

}
