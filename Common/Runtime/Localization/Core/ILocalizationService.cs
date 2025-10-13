using System;

namespace MisterGames.Common.Localization {
    
    public interface ILocalizationService {
        
        event Action<Locale> OnLocaleChanged;
        
        Locale Locale { get; set; }
        
        string GetId(LocalizationKey key);
        string GetId<T>(LocalizationKey<T> key);
        
        string GetLocalizedString(LocalizationKey key);
        
        T GetLocalizedAsset<T>(LocalizationKey<T> key);
        
        string GetLocalizedString(LocalizationKey key, Locale locale);
        
        T GetLocalizedAsset<T>(LocalizationKey<T> key, Locale locale);
    }
    
}