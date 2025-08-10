using System;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    public sealed class LocalizationService : ILocalizationService {
        
        private const bool EnableLogs = true;
        private static readonly string LogPrefix = nameof(LocalizationService).FormatColorOnlyForEditor(Color.white);

        public event Action<Locale> OnLocaleChanged = delegate { };

        public Locale Locale { get => _locale; set => SetLocale(value); }
        
        private LocalizationSettings _settings;
        private Locale _locale;

        public void Initialize(LocalizationSettings settings) {
            _settings = settings;
            
            var defaultLocale = settings.GetLocaleOrFallback(CreateSystemLocale());
            SetLocale(defaultLocale);
        }
        
        private void SetLocale(Locale locale) 
        {
            if (locale == _locale) return;
            
            _locale = locale;
            LogInfo($"set language: {locale}");
            
            OnLocaleChanged.Invoke(locale);
        }
        
        private static Locale CreateSystemLocale() {
            var id = LocaleExtensions.SystemLanguageToLocaleId(Application.systemLanguage);
            return LocaleExtensions.TryGetLocaleById(id, out var locale) ? locale : default;
        }
        
        private static void LogInfo(string message) {
            if (EnableLogs) Debug.Log($"{LogPrefix}: f {Time.frameCount}, {message}");
        }
    }
    
}