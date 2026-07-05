namespace MisterGames.SettingsLib.Base {
    
    public interface ISettingsService {
        
        bool HasUnsavedChanges { get; }
        
        bool TryGet<T>(string key, int index, out T data);
        bool Set<T>(string key, int index, T setting);
    }
    
}