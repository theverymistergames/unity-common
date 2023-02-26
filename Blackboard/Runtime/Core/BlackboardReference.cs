using System;
using UnityEngine;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    internal struct BlackboardReference : IEquatable<BlackboardReference> {

        [SerializeReference] public object value;

        public bool Equals(BlackboardReference other) {
            return Equals(value, other.value);
        }

        public override bool Equals(object obj) {
            return obj is BlackboardReference other && Equals(other);
        }

        public override int GetHashCode() {
            return value != null ? value.GetHashCode() : 0;
        }

        public static bool operator ==(BlackboardReference left, BlackboardReference right) {
            return left.Equals(right);
        }

        public static bool operator !=(BlackboardReference left, BlackboardReference right) {
            return !left.Equals(right);
        }
    }

}
