namespace MisterGames.Common.Localization {
    
    public interface IArgumentResolver {
        void Resolve(LocalizationKey key, Locale locale, ref string value);
    }
    
}