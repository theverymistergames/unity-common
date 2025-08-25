using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Input.Actions {
    
    [Serializable]
    public struct InputActionRef : IEquatable<InputActionRef> {
        
        [SerializeField] internal SerializedGuid _guid;

        public Guid Guid => _guid.ToGuid();
        
        public InputActionRef(Guid guid) {
            _guid = new SerializedGuid(guid);
        }

        public bool Equals(InputActionRef other) => _guid.Equals(other._guid);
        public override bool Equals(object obj) => obj is InputActionRef other && Equals(other);
        public override int GetHashCode() => _guid.GetHashCode();

        public static bool operator ==(InputActionRef left, InputActionRef right) => left.Equals(right);
        public static bool operator !=(InputActionRef left, InputActionRef right) => !left.Equals(right);
    }
    
}