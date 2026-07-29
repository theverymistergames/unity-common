using MisterGames.SettingsLib.Base;

namespace MisterGames.SettingsLib.Descs {
    
    public interface ISettingDescValued<T> : ISettingDesc {
        T GetDefaultValue();
        T GetValue(ISettingsService service, string id);
        void SetValue(ISettingsService service, string id, T value);
    }
    
}