using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MisterGames.Common.Data {

    [Serializable]
    public struct Optional<T> {

        [SerializeField] private T _value;
        [SerializeField] private bool _hasValue;

        public bool HasValue => _hasValue;
        public T Value => _value;

        private Optional(T value, bool hasValue) {
            _value = value;
            _hasValue = hasValue;
        }
        
        public static Optional<T> WithValue(T value) {
            return new Optional<T>(value, true);
        } 
        
        public static Optional<T> Empty() {
            return new Optional<T>(default, false);
        }
        
        public override string ToString() {
            return _hasValue ? $"Optional(value = {_value})" : "Optional(<empty>)";
        }
        
    }

}
