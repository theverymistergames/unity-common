using MisterGames.SettingsLib.Base;

namespace MisterGames.SettingsLib.Descs {
    
    public interface ISettingDescValued<T> : ISettingDesc {
        
        public delegate void Listener(string id, T value);
        void AddListener(Listener listener);
        void RemoveListener(Listener listener);
        
        T GetDefaultValue();
        T GetValue(ISettingsService service, string id);
        void SetValue(ISettingsService service, string id, T value);
    }
    
}