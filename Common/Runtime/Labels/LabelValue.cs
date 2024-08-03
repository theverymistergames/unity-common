using System;

namespace MisterGames.Common.Labels {
    
    [Serializable]
    public struct LabelValue : IEquatable<LabelValue> {
        
        public LabelLibrary library;
        public int array;
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
    
}