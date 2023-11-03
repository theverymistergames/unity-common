namespace MisterGames.Blueprints.Nodes {

    public interface IBlueprintEnableCallback {

        void OnEnable(IBlueprint blueprint, NodeToken token, bool enabled);
    }

}
