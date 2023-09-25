namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintNode {
        void OnCreateNode(IBlueprintStorage storage, int id);
        Port[] CreatePorts(IBlueprintStorage storage, int id);
    }

}
