using System.Runtime.CompilerServices;
using UnityEngine;

namespace MisterGames.Common.Layers {

    public static class LayerMaskExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this LayerMask mask, int layer) {
            return mask == (mask | (1 << layer));
        }
        
        public static bool Intersects(this LayerMask mask0, int mask1) {
            return (mask0 & mask1) != 0;
        }
    }

}
