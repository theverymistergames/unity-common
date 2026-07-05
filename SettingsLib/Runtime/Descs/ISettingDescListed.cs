namespace MisterGames.SettingsLib.Base {
    
    public interface ISettingDescListed : ISettingDesc {
        int GetCount();
        string GetValue(int index);
        int GetIndex(ISettingsService service, string label);
        bool SetIndex(ISettingsService service, string label, int index);
    }
    
}