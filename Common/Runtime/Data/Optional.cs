using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Common.Data {

    [Serializable]
    public struct Optional<T> : IEquatable<Optional<T>> {

        [SerializeField] private bool _hasValue;
        [SerializeField] private T _value;

        public bool HasValue => _hasValue;
        public T Value => _value;

        public static Optional<T> Create(T value = default, bool hasValue = false) {
            return new Optional<T>(value, hasValue);
        }

        public Optional(T value, bool hasValue) {
            _value = value;
            _hasValue = hasValue;
        }

        public T GetOrDefault(T defaultValue) {
            return _hasValue ? _value : defaultValue;
        }

        public bool IsEmptyOrEquals(T value) {
            return !_hasValue || EqualityComparer<T>.Default.Equals(_value, value);
        }

        public bool Equals(Optional<T> other) {
            return !_hasValue && !other._hasValue ||
                   _hasValue && other._hasValue && EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        public override bool Equals(object obj) {
            return obj is Optional<T> other && Equals(other);
        }

        public override int GetHashCode() {
            return _hasValue ? _value.GetHashCode() : 0;
        }

        public static bool operator ==(Optional<T> left, Optional<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(Optional<T> left, Optional<T> right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return $"Optional(value = {_value}, hasValue = {_hasValue})";
        }
    }

}
