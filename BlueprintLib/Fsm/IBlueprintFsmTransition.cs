namespace MisterGames.BlueprintLib {

    public interface IBlueprintFsmTransition {
        bool Arm(IBlueprintFsmState state);
        void Disarm();
    }

}
