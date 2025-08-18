using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    public static class LocaleExtensions {
        
        private static readonly Dictionary<LocaleId, LocaleDescriptor> LocaleIdToDescriptorMap = new() {
            { LocaleId.af, new LocaleDescriptor("af", "Afrikaans", "afrikaans") },
            { LocaleId.ar, new LocaleDescriptor("ar", "Arabic", "الل\u064f\u0651غ\u064eة\u064f الع\u064eر\u064eب\u0650ي\u064e\u0651ة") },
            { LocaleId.eu, new LocaleDescriptor("eu", "Basque", "euskara") },
            { LocaleId.be, new LocaleDescriptor("be", "Belarusian", "беларуская") },
            { LocaleId.bg, new LocaleDescriptor("bg", "Bulgarian", "bŭlgarski") },
            { LocaleId.ca, new LocaleDescriptor("ca", "Catalan", "català") },
            { LocaleId.zh, new LocaleDescriptor("zh", "Chinese", "普通话") },
            { LocaleId.zhCN, new LocaleDescriptor("zh-cn", "Chinese (simplified)", "普通话 (simplified)") },
            { LocaleId.zhTW, new LocaleDescriptor("zh-tw", "Chinese (traditional)", "普通话 (traditional)") },
            { LocaleId.cs, new LocaleDescriptor("cs", "Czech", "Český") },
            { LocaleId.da, new LocaleDescriptor("da", "Danish", "dansk") },
            { LocaleId.nl, new LocaleDescriptor("nl", "Dutch", "nederlandse") },
            { LocaleId.en, new LocaleDescriptor("en", "English", "english") },
            { LocaleId.et, new LocaleDescriptor("et", "Estonian", "eesti keel") },
            { LocaleId.fo, new LocaleDescriptor("fo", "Faroese", "faroese") },
            { LocaleId.fi, new LocaleDescriptor("fi", "Finnish", "suomi") },
            { LocaleId.fr, new LocaleDescriptor("fr", "French", "le français") },
            { LocaleId.de, new LocaleDescriptor("de", "German", "deutsch") },
            { LocaleId.el, new LocaleDescriptor("el", "Greek", "Ελληνικά") },
            { LocaleId.he, new LocaleDescriptor("he", "Hebrew", "ע\u05b4ב\u05b0ר\u05b4ית") },
            { LocaleId.hi, new LocaleDescriptor("hi", "Hindi", "ह\u093fन\u094dद\u0940") },
            { LocaleId.hu, new LocaleDescriptor("hu", "Hungarian", "magyar nyelv") },
            { LocaleId.@is, new LocaleDescriptor("is", "Icelandic", "íslenska") },
            { LocaleId.id, new LocaleDescriptor("id", "Indonesian", "Bahasa Indonesia") },
            { LocaleId.it, new LocaleDescriptor("it", "Italian", "italiano") },
            { LocaleId.ja, new LocaleDescriptor("ja", "Japanese", "日本語") },
            { LocaleId.ko, new LocaleDescriptor("ko", "Korean", "조선말") },
            { LocaleId.lv, new LocaleDescriptor("lv", "Latvian", "latviešu valoda") },
            { LocaleId.lt, new LocaleDescriptor("lt", "Lithuanian", "lietùvių kalbà") },
            { LocaleId.no, new LocaleDescriptor("no", "Norwegian", "norsk") },
            { LocaleId.pl, new LocaleDescriptor("pl", "Polish", "polski") },
            { LocaleId.pt, new LocaleDescriptor("pt", "Portuguese", "portuguesa") },
            { LocaleId.ro, new LocaleDescriptor("ro", "Romanian", "română") },
            { LocaleId.ru, new LocaleDescriptor("ru", "Russian", "русский") },
            { LocaleId.sr, new LocaleDescriptor("sr", "Serbian", "srpski") },
            { LocaleId.sk, new LocaleDescriptor("sk", "Slovak", "slovenský") },
            { LocaleId.sl, new LocaleDescriptor("sl", "Slovenian", "slovenský") },
            { LocaleId.es, new LocaleDescriptor("es", "Spanish", "español") },
            { LocaleId.sv, new LocaleDescriptor("sv", "Swedish", "s svenska") },
            { LocaleId.th, new LocaleDescriptor("th", "Thai", "ภาษาไทย") },
            { LocaleId.tr, new LocaleDescriptor("tr", "Turkish", "Türk dili") },
            { LocaleId.uk, new LocaleDescriptor("uk", "Ukrainian", "українська") },
            { LocaleId.vi, new LocaleDescriptor("vi", "Vietnamese", "tiếng Việt") },
        };

        private static readonly Dictionary<LocaleId, int> LocaleIdToHashMap = new();
        private static readonly Dictionary<int, LocaleId> LocaleHashToIdMap = new();
        private static LocaleId[] LocaleIdArray;
        private static bool _initialized;

        public static IReadOnlyList<LocaleId> LocaleIds => LocaleIdArray;
        
        private static void InitializeMaps() {
            if (_initialized) return;
            
            _initialized = true;
            
            LocaleIdArray = new LocaleId[LocaleIdToDescriptorMap.Count];
            int index = 0;
            
            foreach (var (id, desc) in LocaleIdToDescriptorMap) {
                if (id == LocaleId.Unknown) continue;
                
                int hash = Animator.StringToHash(desc.code);

                LocaleIdToHashMap[id] = hash;
                LocaleHashToIdMap[hash] = id;
                LocaleIdArray[index++] = id;
            }
        }

        public static string FormatLocaleCode(string localeCode) {
            return string.IsNullOrWhiteSpace(localeCode) ? null : localeCode.ToLowerInvariant();
        }
        
        public static bool IsNull(this Locale locale) {
            InitializeMaps();
            
            return locale.hash == 0 || 
                   !LocaleHashToIdMap.ContainsKey(locale.hash) && 
                   (locale.localizationSettings == null || !locale.localizationSettings.IsSupportedLocale(locale.Hash));
        }
        
        public static bool IsNotNull(this Locale locale) {
            return !IsNull(locale);
        }
        
        public static bool TryGetLocaleHashById(LocaleId id, out int hash) {
            InitializeMaps();
            return LocaleIdToHashMap.TryGetValue(id, out hash);
        }
        
        public static bool TryGetLocaleIdByHash(int hash, out LocaleId localeId) {
            InitializeMaps();
            return LocaleHashToIdMap.TryGetValue(hash, out localeId);
        }

        public static bool TryGetLocaleById(LocaleId localeId, out Locale locale) {
            if (TryGetLocaleHashById(localeId, out int hash)) {
                locale = new Locale(hash, localizationSettings: null);
                return true;
            }
            
            locale = default;
            return false;
        }

        public static bool TryGetLocaleDescriptorById(LocaleId id, out LocaleDescriptor localeDescriptor) {
            InitializeMaps();
            return LocaleIdToDescriptorMap.TryGetValue(id, out localeDescriptor);
        }

        public static bool TryGetLocaleDescriptorByHash(int hash, out LocaleDescriptor localeDescriptor) {
            localeDescriptor = default;
            return TryGetLocaleIdByHash(hash, out var id) && 
                   TryGetLocaleDescriptorById(id, out localeDescriptor);
        }
        
        public static LocaleId SystemLanguageToLocaleId(SystemLanguage systemLanguage) {
            return systemLanguage switch {
                SystemLanguage.Afrikaans => LocaleId.af,
                SystemLanguage.Arabic => LocaleId.ar,
                SystemLanguage.Basque => LocaleId.eu,
                SystemLanguage.Belarusian => LocaleId.be,
                SystemLanguage.Bulgarian => LocaleId.bg,
                SystemLanguage.Catalan => LocaleId.ca,
                SystemLanguage.Chinese => LocaleId.zh,
                SystemLanguage.Czech => LocaleId.cs,
                SystemLanguage.Danish => LocaleId.da,
                SystemLanguage.Dutch => LocaleId.nl,
                SystemLanguage.English => LocaleId.en,
                SystemLanguage.Estonian => LocaleId.et,
                SystemLanguage.Faroese => LocaleId.fo,
                SystemLanguage.Finnish => LocaleId.fi,
                SystemLanguage.French => LocaleId.fr,
                SystemLanguage.German => LocaleId.de,
                SystemLanguage.Greek => LocaleId.el,
                SystemLanguage.Hebrew => LocaleId.he,
                SystemLanguage.Hungarian => LocaleId.hu,
                SystemLanguage.Icelandic => LocaleId.@is,
                SystemLanguage.Indonesian => LocaleId.id,
                SystemLanguage.Italian => LocaleId.it,
                SystemLanguage.Japanese => LocaleId.ja,
                SystemLanguage.Korean => LocaleId.ko,
                SystemLanguage.Latvian => LocaleId.lv,
                SystemLanguage.Lithuanian => LocaleId.lt,
                SystemLanguage.Norwegian => LocaleId.no,
                SystemLanguage.Polish => LocaleId.pl,
                SystemLanguage.Portuguese => LocaleId.pt,
                SystemLanguage.Romanian => LocaleId.ro,
                SystemLanguage.Russian => LocaleId.ru,
                SystemLanguage.SerboCroatian => LocaleId.sr,
                SystemLanguage.Slovak => LocaleId.sk,
                SystemLanguage.Slovenian => LocaleId.sl,
                SystemLanguage.Spanish => LocaleId.es,
                SystemLanguage.Swedish => LocaleId.sv,
                SystemLanguage.Thai => LocaleId.th,
                SystemLanguage.Turkish => LocaleId.tr,
                SystemLanguage.Ukrainian => LocaleId.uk,
                SystemLanguage.Vietnamese => LocaleId.vi,
                SystemLanguage.ChineseSimplified => LocaleId.zhCN,
                SystemLanguage.ChineseTraditional => LocaleId.zhTW,
                SystemLanguage.Hindi => LocaleId.hi,
                _ => LocaleId.Unknown,
            };
        }
        
    }
    
}