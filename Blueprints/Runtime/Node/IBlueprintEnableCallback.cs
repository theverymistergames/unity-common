namespace MisterGames.Blueprints.Nodes {

    public interface IBlueprintEnableCallback {

        void OnEnable(IBlueprint blueprint, NodeId id, bool enabled);
    }

}
