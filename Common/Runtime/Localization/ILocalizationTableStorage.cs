namespace MisterGames.Common.Localization {
    
    public interface ILocalizationTableStorage {

        int GetLocaleCount();

        Locale GetLocale(int localeIndex);

        int GetKeyCount();

        string GetKey(int keyIndex);

        string GetValue(int keyIndex, int localeIndex);

        void SetValue(string key, string value, Locale locale);

        void ClearAll();
    }
    
}