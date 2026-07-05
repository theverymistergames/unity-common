using MisterGames.Common.Localization;

namespace MisterGames.SettingsLib.Base {
    
    public interface ISettingDesc {
        LocalizationKey GetName();
        void Initialize(ISettingsService service, string label) { }
        void Deinitialize(ISettingsService service, string label) { }
    }
    
}