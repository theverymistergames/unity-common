namespace MisterGames.Common.Conditions {

    public interface ICondition {
        bool IsMatched { get; }

        void Arm(IConditionCallback callback);
        void Disarm();

        void OnFired();
    }

}
