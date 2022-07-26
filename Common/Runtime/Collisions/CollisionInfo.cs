using UnityEngine;

namespace MisterGames.Common.Collisions {

    public struct CollisionInfo {

        public bool hasContact;
        public Vector3 normal;
        public Vector3 lastHitPoint;
        public Transform surface;

    }

}