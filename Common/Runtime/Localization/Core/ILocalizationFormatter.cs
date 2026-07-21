namespace MisterGames.Common.Localization {
    
    public interface ILocalizationFormatter {
    
        void Format(LocalizationKey key, Locale locale, ref string value);
        
    }
    
}