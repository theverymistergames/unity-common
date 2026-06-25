using System;
using System.Runtime.CompilerServices;

namespace MisterGames.Common.Data {
    
    [Serializable]
    public struct CompareInt {
        
        public CompareMode mode;
        public int value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMatch(int v) {
            return mode.IsMatch(v, value);
        }
    }
    
}