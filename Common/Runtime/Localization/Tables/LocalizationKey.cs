using System;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [Serializable]
    public struct LocalizationKey : IEquatable<LocalizationKey> {
        
        [SerializeField] internal int hash;
        [SerializeField] internal string tableGuid;

        public LocalizationKey(int hash, string tableGuid) {
            this.hash = hash;
            this.tableGuid = tableGuid;
        }
        
        public bool Equals(LocalizationKey other) => hash == other.hash && tableGuid == other.tableGuid;
        public override bool Equals(object obj) => obj is LocalizationKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(hash, tableGuid);
        public static bool operator ==(LocalizationKey left, LocalizationKey right) => left.Equals(right);
        public static bool operator !=(LocalizationKey left, LocalizationKey right) => !left.Equals(right);
    }
    
}