using System;
using System.Diagnostics;
using MisterGames.Common.Collisions;
using MisterGames.Common.Maths;
using MisterGames.Common.Routines;
using MisterGames.Dbg.Draw;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public class CharacterCeilingDetector : CollisionDetector, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        
        [Header("Sphere cast settings")]
        [SerializeField] [Min(1)] private int _maxHits = 2;
        [SerializeField] private float _distance = 1f;
        [SerializeField] private float _radius = 0.5f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        private readonly Vector3 _ceilingDetectionDirection = Vector3.up;
        private Transform _transform;
        private RaycastHit[] _hitsMain;

        private void Awake() {
            _transform = transform;
            _hitsMain = new RaycastHit[_maxHits];
        }

        private void Start() {
            RequestCeiling(forceNotify: true);
        }

        private void OnEnable() {
            _timeDomain.SubscribeUpdate(this);
        }

        private void OnDisable() {
            _timeDomain.UnsubscribeUpdate(this);
        }

        void IUpdate.OnUpdate(float dt) {
            RequestCeiling();
        }

        private void RequestCeiling(bool forceNotify = false) {
            var hitCount = PerformSphereCast(_transform.position, _radius, _distance, _hitsMain);
            
            var hasCeiling = hitCount > 0;
            Vector3 normal;
            Vector3 hitPoint;
            Transform surface = null;

            if (hasCeiling) {
                var hit = _hitsMain[0];
                hitPoint = hit.point;
                normal = hit.normal;
                surface = hit.transform;
            }
            else {
                hitPoint = CollisionInfo.lastHitPoint;
                normal = _ceilingDetectionDirection.Inverted().normalized;
            }
            
            var info = new CollisionInfo {
                hasContact = hasCeiling,
                lastNormal = normal,
                lastHitPoint = hitPoint,
                transform = surface
            };
            
            SetCollisionInfo(info, forceNotify);
        }
        
        private int PerformSphereCast(Vector3 origin, float radius, float distance, RaycastHit[] hits) {
            return Physics.SphereCastNonAlloc(
                origin,
                radius,
                _ceilingDetectionDirection,
                hits,
                distance,
                _layerMask,
                _triggerInteraction
            );
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDrawHitPoint;
        [SerializeField] private bool _debugDrawCast;
        [SerializeField] private bool _debugDrawHasCeilingText;
        [SerializeField] private Vector3 _debugDrawHasCeilingTextOffset;

        private void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            if (_debugDrawHitPoint) {
                if (CollisionInfo.hasContact) {
                    DbgPointer.Create().Position(CollisionInfo.lastHitPoint).Size(0.3f).Color(Color.yellow).Draw();    
                }
            }
            
            if (_debugDrawCast) {
                var start = _transform.position;
                var end = start + _ceilingDetectionDirection * _distance;
                DbgCapsule.Create().From(start).To(end).Radius(_radius).Color(Color.cyan).Draw();
            }
            
            if (_debugDrawHasCeilingText) {
                var text = CollisionInfo.hasContact ? "has ceiling" : "";
                DbgText.Create().Text(text).Position(_transform.position + _debugDrawHasCeilingTextOffset).Draw();
            }
        }
#endif

    }

}
