using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Input.Actions {
    
    [Serializable]
    public struct InputMapRef : IEquatable<InputMapRef> {
        
        [SerializeField] internal SerializedGuid _guid;

        public Guid Guid => _guid.ToGuid();
        
        public InputMapRef(Guid guid) {
            _guid = new SerializedGuid(guid);
        }
        
        public bool Equals(InputMapRef other) => _guid.Equals(other._guid);
        public override bool Equals(object obj) => obj is InputMapRef other && Equals(other);
        public override int GetHashCode() => _guid.GetHashCode();

        public static bool operator ==(InputMapRef left, InputMapRef right) => left.Equals(right);
        public static bool operator !=(InputMapRef left, InputMapRef right) => !left.Equals(right);
    }
    
}