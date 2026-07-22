namespace MisterGames.Common.Localization {
    
    public interface IArgumentResolver {
        void Resolve(Locale locale, ref string value);
    }
    
}