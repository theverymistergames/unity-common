namespace MisterGames.Common.Localization {
    
    public interface ILocalizationFormatter {
        delegate void Lambda(LocalizationKey key, Locale locale, ref string value);
        void Format(LocalizationKey key, Locale locale, ref string value);
    }
    
}