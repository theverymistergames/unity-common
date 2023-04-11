namespace MisterGames.Common.Conditions {
    
    public interface IDynamicDataProvider {
        T GetData<T>() where T : IDynamicData;
    }
    
}
