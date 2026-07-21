using System;

namespace MisterGames.Common.Localization {
    
    public interface ILocalizationService {
        
        event Action<Locale> OnLocaleChanged;
        
        LocalizationSettings Settings { get; }
        Locale Locale { get; set; }
        Locale GetDefaultLocale();
        
        string GetId(LocalizationKey key);
        string GetId<T>(LocalizationKey<T> key);
        
        string GetLocalizedString(LocalizationKey key);
        string GetLocalizedString(LocalizationKey key, Locale locale);
        T GetLocalizedAsset<T>(LocalizationKey<T> key);
        T GetLocalizedAsset<T>(LocalizationKey<T> key, Locale locale);
        
        void RegisterFormatter(ILocalizationFormatter formatter);
        void UnregisterFormatter(ILocalizationFormatter formatter);
    }
    
}