using System;
using System.Collections.Generic;
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
        
        private readonly HashSet<ISettingDescListed.Listener> _listeners = new();

        public void ApplySetting(ISettingsService service, string id) {
            var locale = TryGetSavedLocale(service, id, out var l, out int index)
                ? l
                : GetDefaultLocale(out index);
            
            SetLocale(id, locale, index);
        }
        
        public void ClearSetting(ISettingsService service, string id) {
            service.Remove<string>(id, 0); 
        }
        
        public void ResaveSetting(ISettingsService service, string id) {
            if (service.TryGet(id, 0, out string value)) {
                service.Set(id, 0, value);
            }
        }
        
        public LocalizationKey GetName() {
            return name;
        }

        public void AddListener(ISettingDescListed.Listener listener) {
            _listeners.Add(listener);
        }
        
        public void RemoveListener(ISettingDescListed.Listener listener) {
            _listeners.Remove(listener);
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

        public int GetIndex(ISettingsService service, string id) {
            if (service.TryGet(id, index: 0, out string localeCode) && 
                TryGetIndexOf(LocaleExtensions.CreateLocale(localeCode), out int index)) 
            {
                return index;
            }

            GetDefaultLocale(out index);
            return index >= 0 ? index : 0;
        }

        public bool SetIndex(ISettingsService service, string id, int index) {
            var locale = locales?.GetEntry(index).key ?? GetDefaultLocale(out index);
            bool ok = service.Set(id, index: 0, locale.GetDescriptor().code);
            
            SetLocale(id, locale, index);
            
            return ok;
        }

        private void SetLocale(string id, Locale locale, int index) {
            Services.Get<ILocalizationService>().Locale = locale;

            foreach (var listener in _listeners) {
                listener.Invoke(id, index);
            }
        }
        
        private bool TryGetSavedLocale(ISettingsService service, string id, out Locale locale, out int index) {
            locale = default;
            index = -1;
            
            if (service.TryGet(id, index: 0, out string localeCode) &&
                LocaleExtensions.CreateLocale(localeCode) is var l &&
                TryGetIndexOf(l, out index)) 
            {
                locale = l;
                return true;
            }
            
            index = -1;
            return false;
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