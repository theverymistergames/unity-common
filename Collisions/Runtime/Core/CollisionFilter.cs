using System;
using UnityEngine;

namespace MisterGames.Collisions.Core {

    [Serializable]
    public struct CollisionFilter {

        public float maxDistance;
        public LayerMask layerMask;

        public override string ToString() {
            return $"{nameof(CollisionFilter)}(maxDistance = {maxDistance}, layerMask = {layerMask})";
        }
    }

}
