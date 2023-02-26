using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Blackboards.Core {

    [Serializable]
    internal struct BlackboardValue<T> : IEquatable<BlackboardValue<T>> {

        [SerializeField] public T value;

        public bool Equals(BlackboardValue<T> other) {
            if (typeof(Object).IsAssignableFrom(typeof(T))) return value as Object == other.value as Object;
            return EqualityComparer<T>.Default.Equals(value, other.value);
        }

        public override bool Equals(object obj) {
            return obj is BlackboardValue<T> other && Equals(other);
        }

        public override int GetHashCode() {
            return EqualityComparer<T>.Default.GetHashCode(value);
        }

        public static bool operator ==(BlackboardValue<T> left, BlackboardValue<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(BlackboardValue<T> left, BlackboardValue<T> right) {
            return !left.Equals(right);
        }
    }

}
