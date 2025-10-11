using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public struct LocalizationKey : IEquatable<LocalizationKey> {
        
        [SerializeField] internal int hash;
        [SerializeField] internal SerializedGuid tableGuid;

        public LocalizationKey(int hash, Guid tableGuid) {
            this.hash = hash;
            this.tableGuid = new SerializedGuid(tableGuid);
        }
        
        public bool Equals(LocalizationKey other) => hash == other.hash && tableGuid == other.tableGuid;
        public override bool Equals(object obj) => obj is LocalizationKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(hash, tableGuid);
        public static bool operator ==(LocalizationKey left, LocalizationKey right) => left.Equals(right);
        public static bool operator !=(LocalizationKey left, LocalizationKey right) => !left.Equals(right);
    }
    
    [Serializable]
    public struct LocalizationKey<T> : IEquatable<LocalizationKey<T>> {
        
        [SerializeField] internal int hash;
        [SerializeField] internal SerializedGuid tableGuid;

        public LocalizationKey(int hash, Guid tableGuid) {
            this.hash = hash;
            this.tableGuid = new SerializedGuid(tableGuid);
        }
        
        public bool Equals(LocalizationKey<T> other) => hash == other.hash && tableGuid == other.tableGuid;
        public override bool Equals(object obj) => obj is LocalizationKey<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(hash, tableGuid);
        public static bool operator ==(LocalizationKey<T> left, LocalizationKey<T> right) => left.Equals(right);
        public static bool operator !=(LocalizationKey<T> left, LocalizationKey<T> right) => !left.Equals(right);
    }
    
}