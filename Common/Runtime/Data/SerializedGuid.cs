using System;
using UnityEngine;

namespace MisterGames.Common.Data {
    
    [Serializable]
    public struct SerializedGuid : IEquatable<SerializedGuid> {
        
        [SerializeField] private ulong _guidLow;
        [SerializeField] private ulong _guidHigh;

        public static readonly SerializedGuid Empty = new(0, 0);

        public SerializedGuid(ulong guidLow, ulong guidHigh) {
            _guidLow = guidLow;
            _guidHigh = guidHigh;
        }

        public SerializedGuid(Guid guid) {
            (_guidLow, _guidHigh) = HashHelpers.DecomposeGuid(guid);
        }
        
        public Guid ToGuid() {
            return HashHelpers.ComposeGuid(_guidLow, _guidHigh);
        }

        public override int GetHashCode() {
            return HashHelpers.Combine(_guidLow.GetHashCode(), _guidHigh.GetHashCode());
        }

        public override bool Equals(object obj) {
            return obj is SerializedGuid g && Equals(g);
        }

        public bool Equals(SerializedGuid other) {
            return _guidLow == other._guidLow && _guidHigh == other._guidHigh;
        }

        public static bool operator ==(SerializedGuid lhs, SerializedGuid rhs) {
            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(SerializedGuid lhs, SerializedGuid rhs) {
            return !lhs.Equals(rhs);
        }
        
        public override string ToString() {
            return $"{_guidLow:X16}-{_guidHigh:X16}";
        }

        public string ToString(string format) {
            return ToGuid().ToString(format);
        }

        public string ToString(string format, IFormatProvider provider) {
            return ToGuid().ToString(format, provider);
        }
    }
    
}