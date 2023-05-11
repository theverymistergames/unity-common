using System;
using UnityEngine;

namespace MisterGames.Collisions.Core {

    [Serializable]
    public struct CollisionFilter {

        [Min(0f)] public float maxDistance;
        public LayerMask layerMask;

        public override string ToString() {
            return $"{nameof(CollisionFilter)}(maxDistance = {maxDistance}, layerMask = {layerMask})";
        }
    }

}
