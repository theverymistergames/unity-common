using System;
using MisterGames.Character.Core;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Triggers;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Character.Motion {

    public sealed class CharacterForceZone : MonoBehaviour, IUpdate {

        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        [Header("Force Zone")]
        [SerializeField] private Trigger _enterTrigger;
        [SerializeField] private Trigger _exitTrigger;
        [SerializeField] private Transform _forceSourcePoint;
        [SerializeField] private Vector3 _forceDirection;
        [SerializeField] [Min(0f)] private float _maxDistance;

        [Header("Force Power")]
        [SerializeField] [Min(0f)] private float _forceSmoothFactor;
        [SerializeField] private float _forceMultiplier;
        [SerializeField] private AnimationCurve _forceByDistanceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Force Power Random")]
        [SerializeField] [Range(0f, 1f)] private float _randomAddition;
        [SerializeField] [Min(0f)] private float _randomSmoothFactor;
        [SerializeField] private float _randomMultiplier;
        [SerializeField] private AnimationCurve _randomByDistanceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Obstacles")]
        [SerializeField] private bool _considerObstacles;
        [VisibleIf(nameof(_considerObstacles))]
        [SerializeField] private CollisionDetectorBase _obstacleDetector;
        [VisibleIf(nameof(_considerObstacles))]
        [SerializeField] private CollisionFilter _obstacleCollisionFilter;
        [VisibleIf(nameof(_considerObstacles))]
        [SerializeField] private float _behindObstacleForceMultiplier = 1f;

        public event Action<CharacterAccess> OnEnteredZone = delegate {  };
        public event Action<CharacterAccess> OnExitedZone = delegate {  };
        public event Action<Vector3> OnForceUpdate = delegate {  };

        public float ForceMultiplier { get; set; } = 1f;

        private Transform _characterTransform;
        private Transform _obstacleDetectorTransform;
        private CharacterProcessorMass _characterMass;

        private float _force;
        private float _random;

        private void Awake() {
            if (_considerObstacles) _obstacleDetectorTransform = _obstacleDetector.transform;
        }

        private void OnEnable() {
            _enterTrigger.OnTriggered -= OnEnterTrigger;
            _enterTrigger.OnTriggered += OnEnterTrigger;

            _exitTrigger.OnTriggered -= OnExitTrigger;
            _exitTrigger.OnTriggered += OnExitTrigger;
        }

        private void OnDisable() {
            _enterTrigger.OnTriggered -= OnEnterTrigger;
            _exitTrigger.OnTriggered -= OnExitTrigger;

            _force = 0f;
            _random = 0f;
        }

        private void OnEnterTrigger(Collider go) {
            if (_characterMass != null) return;

            var characterAccess = go.GetComponent<CharacterAccess>();
            if (characterAccess == null) return;

            _characterTransform = characterAccess.transform;
            _characterMass = characterAccess.GetPipeline<ICharacterMotionPipeline>().GetProcessor<CharacterProcessorMass>();

            TimeSources.Get(_playerLoopStage).Subscribe(this);

            OnEnteredZone.Invoke(characterAccess);
        }

        private void OnExitTrigger(Collider go) {
            if (_characterMass == null) return;

            var characterAccess = go.GetComponent<CharacterAccess>();
            if (characterAccess == null) return;

            TimeSources.Get(_playerLoopStage).Unsubscribe(this);

            _characterMass.RemoveForceSource(this);

            _characterMass = null;
            _characterTransform = null;

            _force = 0f;
            _random = 0f;

            OnExitedZone.Invoke(characterAccess);
        }

        public void OnUpdate(float dt) {
            var forceSource = _forceSourcePoint.position;
            var characterPosition = _characterTransform.position;
            var characterToSourceVector = characterPosition - forceSource;

            float distance = characterToSourceVector.magnitude;
            float t = _maxDistance > 0f
                ? 1f - Mathf.Min(1f, distance / _maxDistance)
                : 0f;

            float forceCurveMultiplier = _forceByDistanceCurve.Evaluate(t);
            float forceBase = _forceMultiplier * forceCurveMultiplier;

            float randomBase = Random.Range(-1f, 1f);
            float randomCurveMultiplier = _randomByDistanceCurve.Evaluate(t);
            float targetRandom = _randomAddition * randomBase * _randomMultiplier * randomCurveMultiplier;
            _random = _randomSmoothFactor > 0f
                ? Mathf.Lerp(_random, targetRandom, _randomSmoothFactor * dt)
                : targetRandom;

            float obstacleForceMultiplier = 1f;
            if (_considerObstacles) {
                var obstacleRaycastOriginOffset = Vector3.ProjectOnPlane(characterToSourceVector, _forceDirection);
                var obstacleRaycastOrigin = _obstacleDetectorTransform.position + obstacleRaycastOriginOffset;

                _obstacleDetector.transform.forward = _forceDirection;
                _obstacleDetector.OriginOffset = obstacleRaycastOriginOffset;
                _obstacleDetector.FetchResults();

                var hits = _obstacleDetector.FilterLastResults(_obstacleCollisionFilter);
                if (hits.TryGetMinimumDistanceHit(hits.Length, out var obstacleHit) &&
                    obstacleHit.transform.GetHashCode() != _characterTransform.GetHashCode() &&
                    obstacleHit.distance <= Vector3.Distance(obstacleRaycastOrigin, characterPosition)
                ) {
                    obstacleForceMultiplier = _behindObstacleForceMultiplier;
                }
            }

            float targetForce = (distance <= _maxDistance).AsFloat() *
                                obstacleForceMultiplier *
                                ForceMultiplier *
                                (forceBase + _random);

            _force = _forceSmoothFactor > 0f
                ? Mathf.Lerp(_force, targetForce, _forceSmoothFactor * dt)
                : targetForce;

            var forceVector = _force * _forceDirection;

            _characterMass.ApplyForceSource(this, forceVector);
            OnForceUpdate.Invoke(forceVector);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            if (_forceSourcePoint == null) return;

            var source = _forceSourcePoint.position;
            var target = source + _maxDistance * _forceDirection;

            DebugExt.DrawSphere(source, 0.3f, Color.blue, mode: DebugExt.DrawMode.Gizmo);
            DebugExt.DrawSphere(source, 0.15f, Color.green, mode: DebugExt.DrawMode.Gizmo);
            DebugExt.DrawLine(source, target, Color.green, mode: DebugExt.DrawMode.Gizmo);
        }
#endif
    }

}
