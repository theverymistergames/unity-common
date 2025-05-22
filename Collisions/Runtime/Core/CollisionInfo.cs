using UnityEngine;
using UnityEngine.EventSystems;

namespace MisterGames.Collisions.Core {

    public readonly struct CollisionInfo {

        public static readonly CollisionInfo Empty = new CollisionInfo();

        public readonly bool hasContact;
        public readonly float distance;
        public readonly Vector3 normal;
        public readonly Vector3 point;
        public readonly Rigidbody rigidbody;
        public readonly Collider collider;
        public readonly Transform transform;

        public CollisionInfo(bool hasContact, float distance, Vector3 normal, Vector3 point, Transform transform, Rigidbody rigidbody, Collider collider) {
            this.hasContact = hasContact;
            this.distance = distance;
            this.normal = normal;
            this.point = point;
            this.transform = transform;
            this.rigidbody = rigidbody;
            this.collider = collider;
        }

        public static CollisionInfo FromRaycastHit(RaycastHit raycastHit, bool hasContact = true) {
            return new CollisionInfo(
                hasContact: hasContact,
                raycastHit.distance,
                raycastHit.normal,
                raycastHit.point,
                raycastHit.transform,
                raycastHit.rigidbody,
                raycastHit.collider
            );
        }

        public static CollisionInfo FromRaycastResult(RaycastResult raycastResult, bool hasContact = true) {
            return new CollisionInfo(
                hasContact: hasContact,
                raycastResult.distance,
                raycastResult.worldNormal,
                raycastResult.worldPosition,
                hasContact ? raycastResult.gameObject.transform : null,
                rigidbody: null,
                collider: null
            );
        }

        public override string ToString() {
            return $"CollisionInfo({(hasContact ? $"{distance} to {transform.name} [{point}]" : "None")})";
        }
    }

}
