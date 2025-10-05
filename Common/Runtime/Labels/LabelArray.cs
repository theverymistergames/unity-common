using System;
using MisterGames.Common.Labels.Base;
using UnityEngine;

namespace MisterGames.Common.Labels {
    
    [Serializable]
    public struct LabelArray : IEquatable<LabelArray> {
        
        [SerializeField] internal LabelLibraryBase library;
        [SerializeField] internal int id;

        public LabelArray(LabelLibraryBase library, int id) {
            this.library = library;
            this.id = id;
        }
        
        public bool Equals(LabelArray other) {
            return Equals(library, other.library) && id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is LabelArray other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(library, id);
        }

        public static bool operator ==(LabelArray left, LabelArray right) {
            return left.Equals(right);
        }

        public static bool operator !=(LabelArray left, LabelArray right) {
            return !left.Equals(right);
        }
        
        public override string ToString() {
            return this.GetLabel();
        }
    }
    
    [Serializable]
    public struct LabelArray<T> : IEquatable<LabelArray<T>> {
        
        [SerializeField] internal LabelLibraryBase library;
        [SerializeField] internal int id;

        public LabelArray(LabelLibraryBase library, int id) {
            this.library = library;
            this.id = id;
        }
        
        public bool Equals(LabelArray<T> other) {
            return Equals(library, other.library) && id == other.id;
        }

        public override bool Equals(object obj) {
            return obj is LabelArray<T> other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(library, id);
        }

        public static bool operator ==(LabelArray<T> left, LabelArray<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(LabelArray<T> left, LabelArray<T> right) {
            return !left.Equals(right);
        }
        
        public override string ToString() {
            return this.GetLabel();
        }
    }
    
}