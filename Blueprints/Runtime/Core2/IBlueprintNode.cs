namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintNode {
        void OnCreateNode(IBlueprintNodeDataStorage storage, int id);
        Port[] CreatePorts(IBlueprintNodeDataStorage storage, int id);
    }

}
