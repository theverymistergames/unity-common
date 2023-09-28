namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintEnableDisable2 {
        void OnEnable(IBlueprint blueprint, long id);
        void OnDisable(IBlueprint blueprint, long id);
    }

}
