using System;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public struct LocaleDescriptor : IEquatable<LocaleDescriptor> {
        
        public string code;
        public string description;
        public string nativeDescription;
        
        public LocaleDescriptor(string code, string description, string nativeDescription) {
            this.code = code;
            this.description = description;
            this.nativeDescription = nativeDescription;
        }

        public override string ToString() {
            return $"{nameof(LocaleDescriptor)}({code} {description})";
        }

        public bool Equals(LocaleDescriptor other) => code == other.code;
        public override bool Equals(object obj) => obj is LocaleDescriptor other && Equals(other);
        public override int GetHashCode() => code != null ? code.GetHashCode() : 0;
        public static bool operator ==(LocaleDescriptor left, LocaleDescriptor right) => left.Equals(right);
        public static bool operator !=(LocaleDescriptor left, LocaleDescriptor right) => !left.Equals(right);
    }
    
}