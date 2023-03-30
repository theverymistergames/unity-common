using UnityEngine;

namespace MisterGames.Common.Layers {

    public static class LayerMaskExtensions {

        public static bool Contains(this LayerMask mask, int layer) {
            return mask == (mask | (1 << layer));
        }
    }

}
