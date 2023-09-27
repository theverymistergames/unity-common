namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintNode {
        void OnCreateNode(IBlueprint blueprint, long id);
        Port[] CreatePorts(IBlueprint blueprint, long id);
    }

}
