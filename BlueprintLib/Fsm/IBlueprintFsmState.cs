namespace MisterGames.BlueprintLib {

    public interface IBlueprintFsmState {
        bool TryTransit(IBlueprintFsmTransition transition);
    }

}
