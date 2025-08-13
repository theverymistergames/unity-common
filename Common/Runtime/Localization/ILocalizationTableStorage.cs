namespace MisterGames.Common.Localization {
    
    public interface ILocalizationTableStorage {

        int GetLocaleCount();

        void SetLocalesCount(int count);

        Locale GetLocale(int localeIndex);

        void SetLocale(int localeIndex, Locale locale);

        void ClearLocales();

        int GetKeyCount();

        void SetKeyCount(int count);

        string GetKey(int keyIndex);

        void SetKey(int keyIndex, string key);

        void ClearKeysAndValues();

        string GetValue(int keyIndex, int localeIndex);

        void SetValue(int keyIndex, int localeIndex, string value);
    }
    
}