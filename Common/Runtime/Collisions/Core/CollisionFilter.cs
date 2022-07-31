using System;

namespace MisterGames.Common.Collisions.Core {

    [Serializable]
    public struct CollisionFilter {

        public float maxDistance;

        public override string ToString() {
            return $"{nameof(CollisionFilter)}(maxDistance = {maxDistance})";
        }
    }

}
