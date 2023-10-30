namespace MisterGames.Blueprints.Core {

    public interface IBlueprintPortLinker {
        int GetLinkedPorts(int port, out int count);
    }

}
