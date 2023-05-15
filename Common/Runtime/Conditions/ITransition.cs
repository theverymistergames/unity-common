namespace MisterGames.Common.Conditions {

    public interface ITransition : ICondition {

        void Arm(ITransitionCallback callback);
        void Disarm();
    }
}
