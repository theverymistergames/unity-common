namespace MisterGames.Blueprints.Core2 {

    public interface IBlueprintNodeDataStorage {
        ref T Get<T>(int id) where T : struct;
    }

}
