using System;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    [Serializable]
    public struct LabelValue : IEquatable<LabelValue> {
        
        [SerializeField] internal LabelLibraryBase library;
        [SerializeField] internal int array;
        public int value;

        public bool Equals(LabelValue other) {
            return Equals(library, other.library) && array == other.array && value == other.value;
        }

        public override bool Equals(object obj) {
            return obj is LabelValue other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(library, array, value);
        }

        public static bool operator ==(LabelValue left, LabelValue right) {
            return left.Equals(right);
        }

        public static bool operator !=(LabelValue left, LabelValue right) {
            return !left.Equals(right);
        }
        
        public override string ToString() {
            return this.GetLabel();
        }
    }
    
    [Serializable]
    public struct LabelValue<T> : IEquatable<LabelValue<T>> {
        
        [SerializeField] internal LabelLibraryBaseT<T> library;
        [SerializeField] internal int array;
        public int value;

        public bool Equals(LabelValue<T> other) {
            return Equals(library, other.library) && array == other.array && value == other.value;
        }

        public override bool Equals(object obj) {
            return obj is LabelValue other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(library, array, value);
        }

        public static bool operator ==(LabelValue<T> left, LabelValue<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(LabelValue<T> left, LabelValue<T> right) {
            return !left.Equals(right);
        }
        
        public override string ToString() {
            return this.GetLabel();
        }
    }
    
}