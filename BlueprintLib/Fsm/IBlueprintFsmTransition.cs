namespace MisterGames.BlueprintLib.Fsm {

    public interface IBlueprintFsmTransition {
        void Arm(IBlueprintFsmTransitionCallback callback);
        void Disarm();
    }

}
