namespace MisterGames.SettingsLib.Descs {
    
    public interface ISettingReader<in T> {
        void OnReadValue(T value);
    }
    
}