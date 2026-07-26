using MisterGames.Common.Localization;

namespace MisterGames.SettingsLib.Base {
    
    public interface ISettingDesc {
        LocalizationKey GetName();
        void Initialize(ISettingsService service, string id) { }
        void Deinitialize(ISettingsService service, string id) { }
    }
    
}