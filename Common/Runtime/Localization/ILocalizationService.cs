using System;

namespace MisterGames.Common.Localization {
    
    public interface ILocalizationService {
        
        event Action<Locale> OnLocaleChanged;
        
        Locale Locale { get; set; }
        
    }
    
}