using System;
using UnityEngine;

namespace MisterGames.Common.Data {
    
    [Serializable]
    public struct HashId : IEquatable<HashId> {

        [SerializeField] private int _hash;

#if UNITY_EDITOR
        [SerializeField] private string _name;
#endif
        
        public bool Equals(HashId other) => _hash == other._hash;
        public override bool Equals(object obj) => obj is HashId other && Equals(other);
        public override int GetHashCode() => _hash;

        public static bool operator ==(HashId left, HashId right) => left.Equals(right);
        public static bool operator !=(HashId left, HashId right) => !left.Equals(right);

        public static implicit operator int(HashId hashId) => hashId._hash;

        public override string ToString() {
#if UNITY_EDITOR
            return $"{_name}";
#endif
            return _hash.ToString();
        }
    }
    
}