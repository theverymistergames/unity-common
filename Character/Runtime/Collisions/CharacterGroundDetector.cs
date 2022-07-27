﻿using System;
using MisterGames.Common.Collisions;
using MisterGames.Common.Maths;
using MisterGames.Common.Routines;
using MisterGames.Dbg.Draw;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public class CharacterGroundDetector : CollisionDetector, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;

        [Header("Sphere cast settings")]
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] private float _distanceAddition = 0.02f;
        [SerializeField] private float _radius = 0.5f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Normal calculation")]
        [SerializeField] private float _hitPointElevation = 0.2f;
        [SerializeField] private float _normalSphereCastRadius = 0.05f;

        public Vector3 OriginOffset { get; set; } = Vector3.zero;
        public float Distance { get; set; }
        
        private readonly Vector3 _groundDetectionDirection = Vector3.down;
        private Transform _transform;
        private RaycastHit[] _hitsMain;
        private RaycastHit[] _hitsNormal;

        private void Awake() {
            _transform = transform;
            _hitsMain = new RaycastHit[_maxHits];
            _hitsNormal = new RaycastHit[1];
        }

        private void OnEnable() {
            _timeDomain.SubscribeUpdate(this);
        }

        private void OnDisable() {
            _timeDomain.UnsubscribeUpdate(this);
        }

        private void Start() {
            RequestGround(forceNotify: true);
        }

        void IUpdate.OnUpdate(float dt) {
            RequestGround();
        }

        private void RequestGround(bool forceNotify = false) {
            var origin = GetOrigin();
            int hitCount = PerformSphereCast(origin, _radius, GetDistance(), _hitsMain);
            
            bool isGrounded = hitCount > 0;
            var normal = Vector3.zero;
            var hitPoint = Vector3.zero;
            Transform surface = null;
            
            if (isGrounded) {
                float minSqrMagnitude = -1f;

                for (int i = 0; i < hitCount; i++) {
                    var hit = _hitsMain[i];
                    
                    var point = hit.point;
                    hitPoint += point;

                    float sqrMagnitude = (origin - point).sqrMagnitude;
                    if (sqrMagnitude < minSqrMagnitude || minSqrMagnitude < 0f) {
                        minSqrMagnitude = sqrMagnitude;
                        surface = hit.transform;
                    }

                    if (ClarifyNormalAtPoint(point)) normal += _hitsNormal[0].normal;
                }

                normal = normal.normalized;
                hitPoint /= hitCount;
            }
            else {
                normal = _groundDetectionDirection.Inverted().normalized;
                hitPoint = CollisionInfo.lastHitPoint;
            }

            var info = new CollisionInfo {
                hasContact = isGrounded,
                lastNormal = normal,
                lastHitPoint = hitPoint,
                transform = surface
            };
            
            SetCollisionInfo(info, forceNotify);
        }
        
        private bool ClarifyNormalAtPoint(Vector3 point) {
            var origin = point - _groundDetectionDirection * _hitPointElevation;
            return PerformSphereCast(origin, _normalSphereCastRadius, _hitPointElevation, _hitsNormal) > 0;
        }

        private int PerformSphereCast(Vector3 origin, float radius, float distance, RaycastHit[] hits) {
            return Physics.SphereCastNonAlloc(
                origin, 
                radius,
                _groundDetectionDirection,
                hits, 
                distance,
                _layerMask,
                _triggerInteraction
            );
        }

        private Vector3 GetOrigin() {
            return OriginOffset + _transform.position;
        }

        private float GetDistance() {
            return Distance + _distanceAddition;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDrawNormal;
        [SerializeField] private bool _debugDrawCast;
        [SerializeField] private bool _debugDrawHitPoint;
        [SerializeField] private bool _debugDrawIsGroundedText;
        [SerializeField] private Vector3 _debugDrawIsGroundedTextOffset;
        
        private void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            if (_debugDrawNormal) {
                var start = CollisionInfo.hasContact ? CollisionInfo.lastHitPoint : GetOrigin() + _groundDetectionDirection * (GetDistance() + _radius);
                DbgRay.Create().From(start).Dir(CollisionInfo.lastNormal).Color(Color.blue).Arrow(0.1f).Draw();
            }

            if (_debugDrawHitPoint) {
                if (CollisionInfo.hasContact) {
                    DbgPointer.Create().Position(CollisionInfo.lastHitPoint).Size(0.3f).Color(Color.yellow).Draw();    
                }
            }
            
            if (_debugDrawCast) {
                var start = GetOrigin();
                var end = start + _groundDetectionDirection * GetDistance();
                DbgCapsule.Create().From(start).To(end).Radius(_radius).Color(Color.cyan).Draw();
            }
            
            if (_debugDrawIsGroundedText) {
                string text = CollisionInfo.hasContact ? "grounded" : "in air";
                DbgText.Create().Text(text).Position(GetOrigin() + _debugDrawIsGroundedTextOffset).Draw();
            }
        }
#endif
        
    }

}
