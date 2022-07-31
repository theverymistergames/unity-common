using MisterGames.Common.Collisions;
using MisterGames.Common.Collisions.Core;
using MisterGames.Common.Collisions.Utils;
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
        private RaycastHit[] _hits;
        private int _hitCount;
        private int _lastUpdateFrame = -1;

        private void Awake() {
            _transform = transform;
            _hits = new RaycastHit[_maxHits];
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

        public override void FetchResults() {

        }

        public override void FilterLastResults(CollisionFilter filter, out CollisionInfo info) {
            info = default;

            if (!CollisionInfo.hasContact) return;

            bool hasHit = _hits
                .Filter(_hitCount, filter, out int filterCount)
                .TryGetMinimumDistanceHit(filterCount, out var hit);

            info = new CollisionInfo {
                hasContact = hasHit,
                lastDistance = hit.distance,
                lastNormal = hit.normal,
                lastHitPoint = hit.point,
                transform = hit.transform
            };
        }

        private void RequestCeiling(bool forceNotify = false) {
            int frame = Time.frameCount;
            if (frame == _lastUpdateFrame) return;

            _hitCount = PerformSphereCast(_transform.position, _radius, _distance, _hits);
            bool hasHits = _hitCount > 0;

            Vector3 normal;
            Vector3 hitPoint;
            float hitDistance;
            Transform surface = null;

            if (hasHits) {
                var hit = _hits[0];
                hitPoint = hit.point;
                normal = hit.normal;
                surface = hit.transform;
                hitDistance = hit.distance;
            }
            else {
                hitPoint = CollisionInfo.lastHitPoint;
                normal = _ceilingDetectionDirection.Inverted().normalized;
                hitDistance = CollisionInfo.lastDistance;
            }

            var info = new CollisionInfo {
                hasContact = hasHits,
                lastDistance = hitDistance,
                lastNormal = normal,
                lastHitPoint = hitPoint,
                transform = surface
            };
            
            SetCollisionInfo(info, forceNotify);
            _lastUpdateFrame = frame;
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
                string text = CollisionInfo.hasContact ? "has ceiling" : "";
                DbgText.Create().Text(text).Position(_transform.position + _debugDrawHasCeilingTextOffset).Draw();
            }
        }
#endif

    }

}
