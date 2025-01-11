using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {

    public sealed class RigidbodyForceZone : MonoBehaviour, IUpdate {

        [Header("Force Zone")]
        [SerializeField] private TriggerListenerForRigidbody _triggerListenerForRigidbody;
        [SerializeField] private Transform _forceSourcePoint;
        [SerializeField] private Vector3 _forceRotation;
        [SerializeField] [Min(0f)] private float _maxDistance;
        [SerializeField] private ForceMode _forceMode;

        [Header("Force Power")]
        [SerializeField] private float _forceMultiplier;
        [SerializeField] private AnimationCurve _forceByDistanceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField] [MinMaxSlider(0f, 1f)] private Vector2 _forceByVelocityAngleWeight = new Vector2(0.3f, 1f);

        [Header("Force Power Random")]
        [SerializeField] [Min(0f)] private float _randomMultiplier;
        [SerializeField] private float _randomNoiseSpeed = 1f;
        [SerializeField] private AnimationCurve _randomByDistanceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Obstacles")]
        [SerializeField] private bool _considerObstacles;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] [Min(1)] private int _maxHits = 6;
        [SerializeField] [Min(0f)] private float _distanceOffset = 0.5f;
        [SerializeField] [Min(0f)] private float _behindObstacleForceMultiplier = 0.01f;

        [Header("Disabling")]
        [SerializeField] [Min(0f)] private float _disableDuration = 0.5f;
        
        public event Action<Rigidbody> OnEnterZone = delegate { };
        public event Action<Rigidbody> OnExitZone = delegate { };
        
        private readonly HashSet<Rigidbody> _rigidbodies = new();
        private readonly Dictionary<Rigidbody, float> _rigidbodyForceWeightMap = new();
        private RaycastHit[] _hits;
        private float _forceEnableMul = 1f;
        private byte _disableId;

        private void Awake() {
            _hits = new RaycastHit[_maxHits];
        }

        private void OnEnable() {
            _forceEnableMul = 1f;
            _disableId++;
            
            _triggerListenerForRigidbody.TriggerEnter += OnEnterTrigger;
            _triggerListenerForRigidbody.TriggerExit += OnExitTrigger;
        }

        private void OnDisable() {
            _triggerListenerForRigidbody.TriggerEnter -= OnEnterTrigger;
            _triggerListenerForRigidbody.TriggerExit -= OnExitTrigger;
            
            StartDisabling(destroyCancellationToken).Forget();
        }

        private void OnDestroy() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            ClearZone();
        }

        public float GetForceWeight(Rigidbody rigidbody) {
            return _rigidbodyForceWeightMap.GetValueOrDefault(rigidbody);
        }

        public bool InZone(Rigidbody rigidbody) {
            return _rigidbodies.Contains(rigidbody);
        }

        private async UniTask StartDisabling(CancellationToken cancellationToken) {
            byte id = ++_disableId;
            
            float t = 0f;
            float speed = _disableDuration > 0f ? 1f / _disableDuration : float.MaxValue;
            float startMul = _forceEnableMul;
            
            while (cancellationToken.IsCancellationRequested && t < 1f && id == _disableId) {
                t = Mathf.Clamp01(t + Time.deltaTime * speed);
                _forceEnableMul = Mathf.Lerp(startMul, 0f, t);
                
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            }
            
            if (cancellationToken.IsCancellationRequested || id != _disableId) return;
            
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            ClearZone();
        }

        private void ClearZone() {
            foreach (var rb in _rigidbodies) {
                if (rb != null && rb.gameObject.activeSelf) OnExitZone.Invoke(rb);
            }
            
            _rigidbodies.Clear();
            _rigidbodyForceWeightMap.Clear();
        }
        
        private void OnEnterTrigger(Rigidbody rigidbody) {
            bool added = _rigidbodies.Add(rigidbody);
            _rigidbodyForceWeightMap[rigidbody] = 0f;
            
            if (added) OnEnterZone.Invoke(rigidbody);
            
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnExitTrigger(Rigidbody rigidbody) {
            bool removed = _rigidbodies.Remove(rigidbody);
            _rigidbodyForceWeightMap.Remove(rigidbody);

            if (removed) OnExitZone.Invoke(rigidbody);
            
            if (_rigidbodies.Count == 0) PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var forcePoint = _forceSourcePoint.position;
            var forceDir = Quaternion.Euler(_forceRotation) * _forceSourcePoint.forward;

            foreach (var rb in _rigidbodies) {
                if (rb == null || !rb.gameObject.activeSelf || rb.isKinematic) {
                    _rigidbodyForceWeightMap[rb] = 0f;
                    continue;
                }

                float distance = (rb.position - forcePoint).magnitude;
                float t = _maxDistance > 0f ? 1f - Mathf.Clamp01(distance / _maxDistance) : 0f;

                float forceK = GetMainForce(t);
                float angleK = GetAngleCoeff(forceDir, rb.linearVelocity);
                float randomK = GetRandomForce(t);
                
                float obstacleK = _considerObstacles && DetectObstacle(forcePoint, forceDir, rb, distance) 
                    ? _behindObstacleForceMultiplier 
                    : 1f;
                
                rb.AddForce(_forceEnableMul * (forceK * angleK + randomK) * obstacleK * forceDir, _forceMode);
                
                _rigidbodyForceWeightMap[rb] = _forceMultiplier.IsNearlyZero() 
                    ? 0f 
                    : _forceEnableMul * (forceK + randomK) * obstacleK / _forceMultiplier;
            }
        }

        private float GetMainForce(float t) {
            return _forceMultiplier * _forceByDistanceCurve.Evaluate(t);
        }

        private float GetAngleCoeff(Vector3 forceDir, Vector3 velocity) {
            return Vector3.Dot(forceDir, velocity) >= 0 
                ? _forceByVelocityAngleWeight.x
                : _forceByVelocityAngleWeight.y;
        }

        private float GetRandomForce(float t) {
            float randomBase = (Mathf.PerlinNoise1D(Time.time * _randomNoiseSpeed) - 0.5f) * 2f;
            return _randomMultiplier * randomBase * _randomByDistanceCurve.Evaluate(t);
        }

        private bool DetectObstacle(Vector3 forcePoint, Vector3 forceDir, Rigidbody rb, float distance) {
            var origin = forcePoint + Vector3.ProjectOnPlane(rb.position - forcePoint, forceDir);
            int hitCount = Physics.RaycastNonAlloc(origin, forceDir, _hits, distance + _distanceOffset, _layerMask, QueryTriggerInteraction.Ignore);

            _hits.RemoveInvalidHits(ref hitCount);
            return _hits.TryGetMinimumDistanceHit(hitCount, out var hit) && hit.rigidbody != rb;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void OnDrawGizmos() {
            if (!_showDebugInfo || _forceSourcePoint == null) return;

            var source = _forceSourcePoint.position;
            var target = source + Quaternion.Euler(_forceRotation) * _forceSourcePoint.forward * _maxDistance;

            DebugExt.DrawSphere(source, 0.3f, Color.blue, gizmo: true);
            DebugExt.DrawSphere(target, 0.15f, Color.green, gizmo: true);
            DebugExt.DrawLine(source, target, Color.green, gizmo: true);
        }
#endif
    }

}
