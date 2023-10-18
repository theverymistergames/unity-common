namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintEnableCallback {

        void OnEnable(IBlueprint blueprint, long id, bool enabled);
    }

}
