using UnityEngine;

namespace MisterGames.Common.Collisions {

    public struct CollisionInfo {

        public bool hasContact;
        public Vector3 lastNormal;
        public Vector3 lastHitPoint;
        public Transform transform;

    }

}
