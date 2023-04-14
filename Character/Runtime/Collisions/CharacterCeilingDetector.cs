﻿using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using MisterGames.Dbg.Draw;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public class CharacterCeilingDetector : CollisionDetectorBase, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        
        [Header("Sphere cast settings")]
        [SerializeField] [Min(1)] private int _maxHits = 2;

        [SerializeField] private float _distance = 0.55f;
        [SerializeField] private float _distanceAddition = 0.15f;
        [SerializeField] private float _radius = 0.3f;

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        public Vector3 OriginOffset {
            get => _originOffset;
            set {
                if (_originOffset.IsNearlyEqual(value, tolerance: 0f)) return;

                _originOffset = value;
                _invalidateFlag = true;
            }
        }

        public float Distance {
            get => _distance;
            set {
                if (_distance.IsNearlyEqual(value, tolerance: 0f)) return;

                _distance = value;
                _invalidateFlag = true;
            }
        }

        public float Radius {
            get => _radius;
            set {
                if (_radius.IsNearlyEqual(value, tolerance: 0f)) return;

                _radius = value;
                _invalidateFlag = true;
            }
        }

        private readonly Vector3 _ceilingDetectionDirection = Vector3.up;

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);
        private Transform _transform;
        private RaycastHit[] _hits;
        private int _hitCount;

        private Vector3 _originOffset;
        private int _lastUpdateFrame = -1;
        private bool _invalidateFlag;

        private void Awake() {
            _transform = transform;
            _hits = new RaycastHit[_maxHits];
        }

        private void Start() {
            RequestCeiling(forceNotify: true);
        }

        private void OnEnable() {
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            _timeSource.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            RequestCeiling();
        }

        public override void FetchResults() {
            RequestCeiling();
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
            if (frame == _lastUpdateFrame && !_invalidateFlag) return;

            var origin = OriginOffset + _transform.position;
            float distance = _distance + _distanceAddition;

            _hitCount = PerformSphereCast(origin, _radius, distance, _hits);
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


        [Header("Debug")]
        [SerializeField] private bool _debugDrawHitPoint;
        [SerializeField] private bool _debugDrawCast;
        [SerializeField] private bool _debugDrawHasCeilingText;
        [SerializeField] private Vector3 _debugDrawHasCeilingTextOffset;

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            if (_debugDrawHitPoint) {
                if (CollisionInfo.hasContact) {
                    DbgPointer.Create().Position(CollisionInfo.lastHitPoint).Size(0.3f).Color(Color.yellow).Draw();    
                }
            }
            
            if (_debugDrawCast) {
                var start = transform.position;
                var end = start + _ceilingDetectionDirection * _distance;
                DbgCapsule.Create().From(start).To(end).Radius(_radius).Color(Color.cyan).Draw();
            }
            
            if (_debugDrawHasCeilingText) {
                string text = CollisionInfo.hasContact ? "has ceiling" : "no ceiling";
                DbgText.Create().Text(text).Position(transform.position + _debugDrawHasCeilingTextOffset).Draw();
            }
        }
#endif
    }

}