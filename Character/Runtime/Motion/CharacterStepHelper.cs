using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Collisions.Utils;
using MisterGames.Common;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterStepHelper : MonoBehaviour, IActorComponent, IUpdate {

        [Header("Detection")]
        [SerializeField] private float _sideOffset;
        [SerializeField] private float _lowerRayOffset;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] [Min(0f)] private float _distance;
        [SerializeField] [Min(0f)] private float _minInclineAngle;
        [SerializeField] [Min(0f)] private float _maxStepDepth;
        [SerializeField] [Min(1)] private int _maxHits = 6;
        
        [Header("Resolving")]
        [SerializeField] private bool _disableIfNotUsingGravity;
        [SerializeField] [Min(0f)] private float _maxStepHeight;
        [SerializeField] [Min(0f)] private float _maxStepHeightAir;
        [SerializeField] private Vector3 _climbSpeed;
        [SerializeField] private Vector3 _climbForce;
        [SerializeField] private float _speedMultiplierGround = 1f;
        [SerializeField] private float _speedMultiplierAir = 1f;
        [SerializeField] private float _forceMultiplierGround = 1f;
        [SerializeField] private float _forceMultiplierAir = 1f;
        
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private CharacterGroundDetector _groundDetector;
        private CharacterMotionPipeline _motion;
        private CharacterGravity _characterGravity;
        private Transform _transform;
        private RaycastHit[] _hits;

        void IActorComponent.OnAwake(IActor actor) {
            _transform = actor.Transform;
            _hits = new RaycastHit[_maxHits];
            
            _motion = actor.GetComponent<CharacterMotionPipeline>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _characterGravity = actor.GetComponent<CharacterGravity>();
        }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            if (_disableIfNotUsingGravity && !_characterGravity.HasGravity || _motion.Input == Vector2.zero) return;

            bool isGrounded = _groundDetector.HasContact;
            var up = _transform.up;
            float stepHeight = isGrounded ? _maxStepHeight : _maxStepHeightAir;
            
            var lowerPoint = _transform.position + _lowerRayOffset * up;
            var upperPoint = lowerPoint + stepHeight * up;
            var inputDir = Vector3.ProjectOnPlane(_motion.InputDirWorld, up).normalized;
            
            // Lower ray not detected any obstacles: do nothing
            if (!DoubleRaycast(lowerPoint, inputDir, up, _distance, out var lowerHit)) {
                return;
            }
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawPointer(lowerHit.point, Color.yellow);
#endif

            // Upper ray detected an obstacle: cannot climb up
            if (DoubleRaycast(upperPoint, inputDir, up, lowerHit.distance + _maxStepDepth, out var upperHit)) {
#if UNITY_EDITOR
                if (_showDebugInfo) DebugExt.DrawPointer(upperHit.point, Color.red);
#endif
                return;
            }
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawPointer(upperPoint + inputDir * _distance, Color.green);
#endif

            float angle = Vector3.SignedAngle(up, lowerHit.normal, Vector3.Cross(inputDir, up));
            if (angle < _minInclineAngle) return;

            var speedVector = _climbSpeed.x * inputDir + _climbSpeed.y * up + _climbSpeed.z * -lowerHit.normal; 
            var forceVector = _climbForce.x * inputDir + _climbForce.y * up + _climbForce.z * -lowerHit.normal; 

            float speedK = isGrounded ? _speedMultiplierGround : _speedMultiplierAir;
            float forceK = isGrounded ? _forceMultiplierGround : _forceMultiplierAir;
            
            var diff = speedK * dt * speedVector;
            
            _motion.Position += diff;
            _motion.AddForce(forceVector * forceK, ForceMode.Acceleration);
            
#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawSphere(lowerPoint, 0.01f, Color.green, duration: 5f);
            if (_showDebugInfo) DebugExt.DrawRay(lowerPoint, diff, Color.green, duration: 5f);
#endif
        }
        
        private bool DoubleRaycast(Vector3 origin, Vector3 direction, Vector3 up, float distance, out RaycastHit closestHit) {
            var side = Vector3.Cross(direction, up);
            
            int hitCount = Physics.RaycastNonAlloc(
                origin + side * _sideOffset,
                direction, 
                _hits,
                distance,
                _layerMask,
                QueryTriggerInteraction.Ignore
            );

            bool hasRightHit = _hits.TryGetMinimumDistanceHit(hitCount, out var rightHit);
            
            hitCount = Physics.RaycastNonAlloc(
                origin - side * _sideOffset,
                direction, 
                _hits,
                distance,
                _layerMask,
                QueryTriggerInteraction.Ignore
            );

            bool hasLeftHit = _hits.TryGetMinimumDistanceHit(hitCount, out var leftHit);

#if UNITY_EDITOR
            if (_showDebugInfo) DebugExt.DrawRay(origin + side * _sideOffset, direction * distance, Color.yellow);
            if (_showDebugInfo) DebugExt.DrawRay(origin - side * _sideOffset, direction * distance, Color.yellow);
#endif
            
            if (hasRightHit && hasLeftHit) {
                closestHit = rightHit.distance < leftHit.distance ? rightHit : leftHit;
                return true;
            }

            if (!hasRightHit && !hasLeftHit) {
                closestHit = default;
                return false;
            }

            closestHit = hasRightHit ? rightHit : leftHit;
            return true;
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