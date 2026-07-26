namespace MisterGames.SettingsLib.Base {
    
    public interface ISettingBinder {
        
        void Bind(ISettingsService service, ISettingDesc desc, string id);
        void Unbind();

        void SetupView(ISettingDesc desc);
        void SetupValue(ISettingDesc desc);
    }
    
}