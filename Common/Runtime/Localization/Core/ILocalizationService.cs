using System;

namespace MisterGames.Common.Localization {
    
    public interface ILocalizationService {
        
        event Action<Locale> OnLocaleChanged;
        
        Locale Locale { get; set; }
        
        string GetLocalizedString(LocalizationKey key);
        
        T GetLocalizedAsset<T>(LocalizationKey<T> key);
    }
    
}