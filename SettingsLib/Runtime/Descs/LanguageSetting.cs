using System;
using MisterGames.Common.Data;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.SettingsLib.Base;

namespace MisterGames.SettingsLib.Descs {
    
    [Serializable]
    public sealed class LanguageSetting : ISettingDescListed {

        public LocalizationKey name;
        public DefaultLocaleMode defaultLocaleMode;
        public SerializedDictionary<Locale, LocalizationKey> locales;

        public enum DefaultLocaleMode {
            FirstLocaleInList,
            Auto,
        }

        public void Initialize(ISettingsService service, string label) {
            if (service.TryGet(label, index: 0, out string localeCode) &&
                LocaleExtensions.CreateLocale(localeCode) is var locale &&
                TryGetIndexOf(locale, out _)) 
            {
                Services.Get<ILocalizationService>().Locale = locale;
                return;
            }

            Services.Get<ILocalizationService>().Locale = GetDefaultLocale(out _);
        }

        public LocalizationKey GetName() {
            return name;
        }

        public int GetCount() {
            return locales?.Count ?? 0;
        }

        public string GetValue(int index) {
            if (index < 0 || index >= locales?.Count) {
                return $"<unsupported locale index [{index}]>";
            }

            return locales!.GetEntry(index).value.GetValue();
        }

        public int GetIndex(ISettingsService service, string label) {
            if (service.TryGet(label, index: 0, out string localeCode) && 
                TryGetIndexOf(LocaleExtensions.CreateLocale(localeCode), out int index)) 
            {
                return index;
            }

            GetDefaultLocale(out index);
            return index >= 0 ? index : 0;
        }

        public bool SetIndex(ISettingsService service, string label, int index) {
            var locale = locales?.GetEntry(index).key ?? GetDefaultLocale(out _);
            bool ok = service.Set(label, index: 0, locale.GetDescriptor().code);

            Services.Get<ILocalizationService>().Locale = locale;
            
            return ok;
        }

        private Locale GetDefaultLocale(out int index) {
            index = -1;
            
            switch (defaultLocaleMode) {
                case DefaultLocaleMode.FirstLocaleInList:
                    if (locales is not { Count: > 0 }) {
                        return Services.Get<ILocalizationService>().GetDefaultLocale();
                    }
                    
                    index = 0;
                    return locales.GetEntry(0).key;
                
                case DefaultLocaleMode.Auto:
                    var locale = Services.Get<ILocalizationService>().GetDefaultLocale();
                    if (TryGetIndexOf(locale, out int i)) {
                        index = i;
                    }
                    
                    return locale;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private bool TryGetIndexOf(Locale locale, out int index) {
            index = locales.FirstIndexOf(locale, (l, e) => e.key == l);
            return index >= 0;
        }
    }
    
}