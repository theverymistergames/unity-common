namespace MisterGames.SettingsLib.Base {
    
    public interface ISettingBinder {
        
        void Bind(ISettingsService service, ISettingDesc desc, string label);
        void Unbind();

        void SetupView(ISettingDesc desc);
        void SetupValue();
    }
    
}