using System;
using MisterGames.Collisions.Core;
using MisterGames.Dbg.Draw;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public class CharacterControllerHitDetector : CollisionDetectorBase {

        public override int Capacity => 0;

        public override void FetchResults() { }

        public override ReadOnlySpan<CollisionInfo> FilterLastResults(CollisionFilter filter) {
            return ReadOnlySpan<CollisionInfo>.Empty;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit) {
            if (!enabled) return;

            var info = new CollisionInfo(hasContact: true, distance: 0f, hit.normal, hit.point, hit.transform);
            SetCollisionInfo(info, forceNotify: true);
        }

        [Header("Debug")]
        [SerializeField] private bool _debugDrawHitPoint;

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            if (_debugDrawHitPoint) {
                if (CollisionInfo.hasContact) {
                    DbgPointer.Create().Position(CollisionInfo.point).Size(0.3f).Color(Color.yellow).Draw();    
                }
            }
        }
#endif
    }

}
