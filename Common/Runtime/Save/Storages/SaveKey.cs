using System;

namespace MisterGames.Common.Save.Tables {
    
    [Serializable]
    public struct SaveKey : IEquatable<SaveKey> {
        
        public string id;
        public int index;

        public SaveKey(string id, int index) {
            this.id = id;
            this.index = index;
        }
        
        public bool Equals(SaveKey other) => id == other.id && index == other.index;
        public override bool Equals(object obj) => obj is SaveKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(id, index);
        public static bool operator ==(SaveKey left, SaveKey right) => left.Equals(right);
        public static bool operator !=(SaveKey left, SaveKey right) => !left.Equals(right);
    }
    
}