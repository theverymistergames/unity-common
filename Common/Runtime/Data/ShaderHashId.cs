using System;
using UnityEngine;

namespace MisterGames.Common.Data {
    
    [Serializable]
    public struct ShaderHashId : IEquatable<ShaderHashId> {
        
        [SerializeField] private string _name;

        private int _hash;
        
        public bool Equals(ShaderHashId other) => _name == other._name;
        public override bool Equals(object obj) => obj is HashId other && Equals(other);
        public override int GetHashCode() => _name.GetHashCode();

        public static bool operator ==(ShaderHashId left, ShaderHashId right) => left.Equals(right);
        public static bool operator !=(ShaderHashId left, ShaderHashId right) => !left.Equals(right);

        public static implicit operator int(ShaderHashId hashId) {
            if (hashId._hash == 0) hashId._hash = Shader.PropertyToID(hashId._name);
            return hashId._hash;
        }

        public override string ToString() {
            return _name;
        }
    }
    
}