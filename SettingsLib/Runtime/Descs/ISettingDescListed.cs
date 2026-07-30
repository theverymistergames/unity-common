namespace MisterGames.SettingsLib.Base {
    
    public interface ISettingDescListed : ISettingDesc {
        public delegate void Listener(string id, int index);
        void AddListener(Listener listener);
        void RemoveListener(Listener listener);
        
        int GetCount();
        string GetValue(int index);
        int GetIndex(ISettingsService service, string id);
        bool SetIndex(ISettingsService service, string id, int index);
    }
    
}