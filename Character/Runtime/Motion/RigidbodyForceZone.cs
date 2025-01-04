using System.Collections.Generic;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Motion {

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
        [SerializeField] [Min(0f)] private float _behindObstacleForceMultiplier = 0.01f;

        private readonly HashSet<Rigidbody> _rigidbodies = new();
        private RaycastHit[] _hits;

        private void Awake() {
            _hits = new RaycastHit[_maxHits];
        }

        private void OnEnable() {
            _triggerListenerForRigidbody.TriggerEnter += OnEnterTrigger;
            _triggerListenerForRigidbody.TriggerExit += OnExitTrigger;
        }

        private void OnDisable() {
            _triggerListenerForRigidbody.TriggerEnter -= OnEnterTrigger;
            _triggerListenerForRigidbody.TriggerExit -= OnExitTrigger;
            
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        private void OnEnterTrigger(Rigidbody rigidbody) {
            _rigidbodies.Add(rigidbody);
            
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnExitTrigger(Rigidbody rigidbody) {
            _rigidbodies.Remove(rigidbody);
            
            if (_rigidbodies.Count == 0) PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var forcePoint = _forceSourcePoint.position;
            var forceDir = Quaternion.Euler(_forceRotation) * _forceSourcePoint.forward;

            foreach (var rb in _rigidbodies) {
                if (!rb.gameObject.activeSelf) continue;
                
                var force = GetForce(forcePoint, forceDir, rb.position, rb.linearVelocity);
                float obstacleK = _considerObstacles && DetectObstacle(forcePoint, forceDir, rb) 
                    ? _behindObstacleForceMultiplier 
                    : 1f;
                
                rb.AddForce(force * obstacleK, _forceMode);
            }
        }

        private Vector3 GetForce(Vector3 forcePoint, Vector3 forceDir, Vector3 point, Vector3 velocity) {
            float distance = (point - forcePoint).magnitude;
            float t = _maxDistance > 0f ? 1f - Mathf.Clamp01(distance / _maxDistance) : 0f;
            
            float forceK = _forceMultiplier * _forceByDistanceCurve.Evaluate(t);
            float angleK = Vector3.Dot(forceDir, velocity) >= 0
                ? _forceByVelocityAngleWeight.x
                : _forceByVelocityAngleWeight.y;

            float randomBase = (Mathf.PerlinNoise1D(Time.time * _randomNoiseSpeed) - 0.5f) * 2f;
            float randomK = randomBase * _randomMultiplier * _randomByDistanceCurve.Evaluate(t);

            return forceDir * (angleK * forceK + randomK);
        }

        private bool DetectObstacle(Vector3 forcePoint, Vector3 forceDir, Rigidbody rb) {
            var origin = forcePoint + Vector3.ProjectOnPlane(rb.position - forcePoint, forceDir);
            int hitCount = Physics.RaycastNonAlloc(origin, forceDir, _hits, _maxDistance, _layerMask, QueryTriggerInteraction.Ignore);

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
