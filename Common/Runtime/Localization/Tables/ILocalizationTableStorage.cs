namespace MisterGames.Common.Localization {
    
    public interface ILocalizationTableStorage {

        int GetLocaleCount();

        Locale GetLocale(int localeIndex);

        int GetKeyCount();

        bool TryGetKey(int keyHash, out string key);
        
        string GetKey(int keyIndex);

        void ClearAll();
    }
    
    public interface ILocalizationTableStorage<T> : ILocalizationTableStorage {

        bool TryGetValue(int keyIndex, int localeIndex, out T value);

        void SetValue(string key, T value, Locale locale);
    }
    
}