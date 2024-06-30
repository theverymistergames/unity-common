using MisterGames.Actors;
using MisterGames.Character.Collisions;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterStepHelper : MonoBehaviour, IActorComponent, IUpdate {
        
        [Header("Detection")]
        [SerializeField] private float _lowerRayOffset;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] [Min(0f)] private float _distance;
        [SerializeField] [Min(0f)] private float _minInclineAngle;
        [SerializeField] [Min(0f)] private float _maxStepDepth;
        [SerializeField] [Min(1)] private int _maxHits = 6;
        
        [Header("Resolving")]
        [SerializeField] [Min(0f)] private float _maxStepHeight;
        [SerializeField] [Min(0f)] private float _maxStepHeightAir;
        [SerializeField] private Vector2 _climbSpeed;
        [SerializeField] private float _airSpeedMultiplier = 1f;
        
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private CharacterGroundDetector _groundDetector;
        private CharacterMotionPipeline _motion;
        private Transform _transform;
        private RaycastHit[] _hits;

        void IActorComponent.OnAwake(IActor actor)
        {
            _transform = actor.Transform;
            _hits = new RaycastHit[_maxHits];
            
            _motion = actor.GetComponent<CharacterMotionPipeline>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
        }

        private void OnEnable()
        {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable()
        {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        public void OnUpdate(float dt) {
            if (_motion.MotionInput.IsNearlyZero()) return;

            _groundDetector.FetchResults();
            bool isGrounded = _groundDetector.CollisionInfo.hasContact;
            
            var up = _transform.up;
            float stepHeight = isGrounded ? _maxStepHeight : _maxStepHeightAir;
            
            var lowerPoint = _transform.position + _lowerRayOffset * up + _motion.Velocity * dt;
            var upperPoint = lowerPoint + stepHeight * up;
            var motionDir = _motion.MotionDirWorld.normalized;
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawRay(lowerPoint, motionDir * _distance, Color.yellow);
#endif
            
            // Lower ray not detected any obstacles: do nothing
            if (!Raycast(lowerPoint, motionDir, _distance, out var lowerHit)) return;
      
            float maxUpperDistance = lowerHit.distance + _maxStepDepth;
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawPointer(lowerHit.point, Color.yellow, 0.1f);
            if (_showDebugInfo) DebugExt.DrawRay(upperPoint, motionDir * maxUpperDistance, Color.yellow);
#endif
            
            // Upper ray detected an obstacle: cannot climb up
            if (Raycast(upperPoint, motionDir, maxUpperDistance, out var upperHit))
            {
#if UNITY_EDITOR
                if (_showDebugInfo) DebugExt.DrawPointer(upperHit.point, Color.red, 0.1f);
#endif
                return;
            }
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawPointer(upperPoint + motionDir * _distance, Color.green, 0.1f);
#endif

            float angle = Vector3.SignedAngle(up, lowerHit.normal, Vector3.Cross(motionDir, up));
            if (angle < _minInclineAngle) return;

            float speed = isGrounded ? 1f : _airSpeedMultiplier;
            var delta = (_climbSpeed.x * motionDir + _climbSpeed.y * up) * (speed * dt);
            
            _motion.Position += delta;
            
#if UNITY_EDITOR
            if (_showDebugInfo)
            {
                DebugExt.DrawSphere(lowerPoint, 0.01f, Color.green, duration: 5f);
                DebugExt.DrawRay(lowerPoint, delta, Color.green, duration: 5f);
            }
#endif
        }

        private bool Raycast(Vector3 origin, Vector3 direction, float distance, out RaycastHit closestHit) {
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                direction, 
                _hits,
                distance,
                _layerMask,
                QueryTriggerInteraction.Ignore
            );

            return _hits.TryGetMinimumDistanceHit(hitCount, out closestHit);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (Application.isPlaying || !_showDebugInfo) return;
            
            var tr = transform;
            var up = tr.up;
            var moveDirection = tr.right;
            var pos = tr.position;
            var lowerPoint = pos + _lowerRayOffset * up;
            var upperPoint = lowerPoint + _maxStepHeight * up;
            var upperPointAir = lowerPoint + _maxStepHeightAir * up;
                
            DebugExt.DrawRay(upperPoint, moveDirection * (_distance + _maxStepDepth), Color.yellow, gizmo: true);
            DebugExt.DrawRay(upperPointAir, moveDirection * (_distance + _maxStepDepth), Color.magenta, gizmo: true);
            DebugExt.DrawRay(lowerPoint, moveDirection * _distance, Color.yellow, gizmo: true);
        }
#endif    
    }
    
}