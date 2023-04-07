using MisterGames.Collisions.Core;
using MisterGames.Dbg.Draw;
using UnityEngine;

namespace MisterGames.Character.Core2.Collisions {

    public class CharacterControllerHitDetector : CollisionDetectorBase {

        public override void FetchResults() { }

        public override void FilterLastResults(CollisionFilter filter, out CollisionInfo info) {
            info = default;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit) {
            var info = new CollisionInfo {
                hasContact = true,
                lastDistance = 0f,
                lastNormal = hit.normal,
                lastHitPoint = hit.point,
                transform = hit.transform
            };
            SetCollisionInfo(info, forceNotify: true);
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDrawHitPoint;

        private void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            if (_debugDrawHitPoint) {
                if (CollisionInfo.hasContact) {
                    DbgPointer.Create().Position(CollisionInfo.lastHitPoint).Size(0.3f).Color(Color.yellow).Draw();    
                }
            }
        }
#endif
    }

}
