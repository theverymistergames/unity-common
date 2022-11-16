using UnityEngine;

namespace MisterGames.Collisions.Core {

    public struct CollisionInfo {

        public bool hasContact;
        public float lastDistance;
        public Vector3 lastNormal;
        public Vector3 lastHitPoint;
        public Transform transform;

        public override string ToString() {
            string content = hasContact ? $"{lastDistance} to {transform.name} [{lastHitPoint}]" : "None";
            return $"CollisionInfo({content})";
        }
    }

}
