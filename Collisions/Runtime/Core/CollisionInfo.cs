using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.Collisions.Core {

    public readonly struct CollisionInfo {

        public static readonly CollisionInfo Empty = new CollisionInfo();

        public readonly bool hasContact;
        public readonly float distance;
        public readonly Vector3 normal;
        public readonly Vector3 point;
        public readonly Transform transform;

        public CollisionInfo(bool hasContact, float distance, Vector3 normal, Vector3 point, Transform transform) {
            this.hasContact = hasContact;
            this.distance = distance;
            this.normal = normal;
            this.point = point;
            this.transform = transform;
        }

        public static CollisionInfo FromRaycastHit(RaycastHit raycastHit, bool hasContact = true) {
            return new CollisionInfo(
                hasContact: hasContact,
                raycastHit.distance,
                raycastHit.normal,
                raycastHit.point,
                raycastHit.transform
            );
        }

        public static CollisionInfo FromRaycastResult(RaycastResult raycastResult, bool hasContact = true) {
            return new CollisionInfo(
                hasContact: hasContact,
                raycastResult.distance,
                raycastResult.worldNormal,
                raycastResult.worldPosition,
                hasContact ? raycastResult.gameObject.transform : null
            );
        }

        public override string ToString() {
            string content = hasContact ? $"{distance} to {transform.name} [{point}]" : "None";
            return $"CollisionInfo({content})";
        }
    }

}
