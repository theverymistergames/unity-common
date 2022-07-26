using System.Diagnostics;
using MisterGames.Common.Collisions;
using MisterGames.Dbg.Draw;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public class CharacterControllerHitDetector : CollisionDetector {
        
        private void OnControllerColliderHit(ControllerColliderHit hit) {
            var info = new CollisionInfo {
                hasContact = true,
                normal = hit.normal,
                lastHitPoint = hit.point,
                surface = hit.transform
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