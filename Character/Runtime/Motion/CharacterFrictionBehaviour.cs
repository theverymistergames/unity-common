using MisterGames.Actors;
using MisterGames.Common;
using MisterGames.Common.Tick;
using Unity.Collections;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterFrictionBehaviour  : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] [Min(0f)] private float _frictionGrounded = 0.6f;
        [SerializeField] [Min(0f)] private float _frictionSlope = 1f;
        [SerializeField] [Min(0f)] private float _frictionSlopeOverMaxAngle = 0.6f;
        [SerializeField] private float _minDotProduct = -0.001f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private Transform _transform;
        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        private CharacterMotionPipeline _motion;
        private int _colliderInstanceId;

        private Vector3 _up;
        private Vector3 _lowerPoint;
        private Vector2 _slopeAngleLimits;
        private float _slopeAngle;
        private float _friction;

        void IActorComponent.OnAwake(IActor actor) {
            _motion = actor.GetComponent<CharacterMotionPipeline>();
            _rigidbody = actor.GetComponent<Rigidbody>();
            _transform = _rigidbody.transform;
            
            _collider = actor.GetComponent<CapsuleCollider>();
            _collider.hasModifiableContacts = true;
            _colliderInstanceId = _collider.GetInstanceID();
            
            SetupFriction();
        }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);
            
            Physics.ContactModifyEvent += OnContactModifyEvent;
            Physics.ContactModifyEventCCD += OnContactModifyEventCCD;
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            
            Physics.ContactModifyEvent -= OnContactModifyEvent;
            Physics.ContactModifyEventCCD -= OnContactModifyEventCCD;
        }

        void IUpdate.OnUpdate(float dt) {
            _up = _transform.up;
            _lowerPoint = _transform.TransformPoint(_collider.center - Vector3.up * (_collider.height * 0.5f - _collider.radius));
            
            _slopeAngle = _motion.SlopeAngle;
            _slopeAngleLimits = _motion.SlopeAngleLimits;
            _friction = GetFriction();
        }

        private void SetupFriction() {
            if (_collider.material == null) _collider.material = new PhysicsMaterial();
            
            var mat = _collider.material;
            
            mat.dynamicFriction = _frictionGrounded;
            mat.staticFriction = _frictionGrounded;
        }
        
        private void OnContactModifyEvent(PhysicsScene arg1, NativeArray<ModifiableContactPair> pairs) {
            ModifyContacts(pairs);
        }

        private void OnContactModifyEventCCD(PhysicsScene arg1, NativeArray<ModifiableContactPair> pairs) {
            ModifyContacts(pairs);
        }

        private void ModifyContacts(NativeArray<ModifiableContactPair> pairs) {
            for (int i = 0; i < pairs.Length; i++) {
                var pair = pairs[i];

                if (pair.colliderInstanceID != _colliderInstanceId && pair.otherColliderInstanceID != _colliderInstanceId) {
                    continue;
                }

#if UNITY_EDITOR
                if (_showDebugInfo) DebugExt.DrawSphere(_lowerPoint, 0.05f, Color.green);
#endif

                for (int j = 0; j < pair.contactCount; j++) {
#if UNITY_EDITOR
                    var col = Vector3.Dot(pair.GetPoint(j) - _lowerPoint, _up) >= _minDotProduct ? Color.red : Color.yellow; 
                    if (_showDebugInfo) DebugExt.DrawSphere(pair.GetPoint(j), 0.03f, col);
                    if (_showDebugInfo) DebugExt.DrawLine(pair.GetPoint(j), _lowerPoint, col);
#endif
                    
                    // Contact above capsule lower point.
                    if (Vector3.Dot(pair.GetPoint(j) - _lowerPoint, _up) >= _minDotProduct) {
                        pair.SetDynamicFriction(j, 0f);
                        pair.SetStaticFriction(j, 0f);
                        continue;
                    }
                    
                    pair.SetDynamicFriction(j, _friction);
                    pair.SetStaticFriction(j, _friction);
                }
            }
        }

        private float GetFriction() {
            float absAngle = Mathf.Abs(_slopeAngle);
            return absAngle < _slopeAngleLimits.x ? _frictionGrounded 
                : absAngle <= _slopeAngleLimits.y ? _frictionSlope 
                : _frictionSlopeOverMaxAngle;
        }
    }
    
}