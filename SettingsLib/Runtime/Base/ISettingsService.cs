using System.Collections.Generic;

namespace MisterGames.SettingsLib.Base {
    
    public interface ISettingsService {
        
        bool HasUnsavedChanges { get; }
        
        bool TryGet<T>(string key, int index, out T data);
        bool Set<T>(string key, int index, T setting);
        bool Remove<T>(string key, int index);
        
        public void SaveSettings();
        public void RevertToDefaultSettings();
        public void RevertToLastSavedSettings();
        public void RevertToDefaultSettings(HashSet<string> keys);
        public void RevertToLastSavedSettings(HashSet<string> keys);
    }
    
}