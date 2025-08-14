using System;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public struct Locale : IEquatable<Locale> {
        
        [SerializeField] internal int hash;
        [SerializeField] internal LocalizationSettings localizationSettings;

        private const string Unknown = "<unknown>";
        
        public int Hash => hash;

        internal Locale(int hash, LocalizationSettings localizationSettings) {
            this.hash = hash;
            this.localizationSettings = localizationSettings;
        }
        
        public LocaleDescriptor GetDescriptor() {
            LocaleDescriptor descriptor;
            
            if (localizationSettings == null) {
                return LocaleExtensions.TryGetLocaleDescriptorByHash(hash, out descriptor) ? descriptor : default;
            }
            
            return localizationSettings.TryGetLocaleDescriptorByHash(hash, out descriptor) ? descriptor : default;
        }
        
        public bool Equals(Locale other) => hash == other.hash;
        public override bool Equals(object obj) => obj is Locale other && Equals(other);
        public override int GetHashCode() => hash;
        public static bool operator ==(Locale left, Locale right) => left.Equals(right);
        public static bool operator !=(Locale left, Locale right) => !left.Equals(right);

        public override string ToString() {
            var desc = GetDescriptor();
            return string.IsNullOrWhiteSpace(desc.code) ? Unknown : $"[{desc.code} ({desc.description})]";
        }
    }
    
}