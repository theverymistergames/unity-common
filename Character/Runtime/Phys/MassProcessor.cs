using System;
 using MisterGames.Character.Configs;
 using MisterGames.Collisions.Core;
 using MisterGames.Common.Data;
 using MisterGames.Common.Maths;
 using MisterGames.Dbg.Draw;
 using MisterGames.Tick.Core;
 using UnityEngine;
 using Object = UnityEngine.Object;

 namespace MisterGames.Character.Phys {

    public class MassProcessor : MonoBehaviour, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [SerializeField] private CollisionDetector _groundDetector;
        [SerializeField] private CollisionDetector _ceilingDetector;
        [SerializeField] private CollisionDetector _hitDetector;
        [SerializeField] private MassSettings _massSettings;

        public event Action OnFell = delegate {  };
        public event Action<Vector3> OnLanded = delegate {  };
        
        public Vector3 Velocity { get; private set; }
        public bool IsGrounded { get; private set; }
        
        private readonly Vector3 _gravityDirection = Vector3.down;
        private Vector3 _gravityComp;
        private Vector3 _inertiaComp;
        private Vector3 _forceComp;
        private Vector3 _targetInertia;

        private bool _isGravityEnabled;

        private readonly ObjectDataMap<Vector3> _forces = new ObjectDataMap<Vector3>();
        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);

        public void EnableGravity(bool isEnabled) {
            _isGravityEnabled = isEnabled;
            if (_isGravityEnabled) return;

            _gravityComp = Vector3.zero;
            _inertiaComp = Vector3.zero;
            _targetInertia = Vector3.zero;
        }

        public void RegisterForceSource(Object source) {
            _forces.Register(source, Vector3.zero);
        }
        
        public void UnregisterForceSource(Object source) {
            _forceComp -= _forces.Get(source);
            _forces.Unregister(source);
        }
        
        public void SetForce(Object source, Vector3 force) {
            _forceComp -= _forces.Get(source);
            _forces.Set(source, force);
            _forceComp += force;
        }
        
        public void ApplyImpulse(Vector3 impulse) {
            if (!_isGravityEnabled) return;

            _targetInertia += impulse.WithY(0);
            if (impulse.y > 0 && !_ceilingDetector.CollisionInfo.hasContact || impulse.y < 0 && !IsGrounded) {
                _gravityComp.y += impulse.y;    
            }
        }
        
        public void ApplyVelocityChange(Vector3 impulse) {
            if (!_isGravityEnabled) return;

            _targetInertia = impulse.WithY(0);
            if (impulse.y > 0 && !_ceilingDetector.CollisionInfo.hasContact || impulse.y < 0 && !IsGrounded) {
                _gravityComp.y = impulse.y;    
            }
        }

        public void ResetAllForces() {
            _gravityComp = Vector3.zero;
            _inertiaComp = Vector3.zero;
            _forceComp = Vector3.zero;
            _targetInertia = Vector3.zero;
        }

        private void OnEnable() {
            _groundDetector.OnLostContact += HandleFell;
            _groundDetector.OnContact += HandleLanded;
            _ceilingDetector.OnContact += HandleCeilingAppeared;
            _hitDetector.OnContact += HandleColliderHit;
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            _forces.Clear();
            _groundDetector.OnLostContact -= HandleFell;
            _groundDetector.OnContact -= HandleLanded;
            _ceilingDetector.OnContact -= HandleCeilingAppeared;
            _hitDetector.OnContact -= HandleColliderHit;
            _timeSource.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            CheckGround();
            
            if (IsGrounded) UpdateGrounded(dt);
            else UpdateInAir(dt);
            
            Velocity = GetVelocity();
        }

        private void CheckGround() {
            bool wasGrounded = IsGrounded;
            IsGrounded = _groundDetector.CollisionInfo.hasContact;
            
            if (wasGrounded && !IsGrounded) {
                HandleFell();
                return;
            }

            if (!wasGrounded && IsGrounded) {
                HandleLanded();
            }
        }

        private void HandleFell() {
            if (_groundDetector.CollisionInfo.hasContact) return;

            if (_isGravityEnabled) {
                _gravityComp.y += Velocity.y;
                _targetInertia = Velocity.WithY(0);
            }

            OnFell.Invoke();
        }

        private void HandleLanded() {
            _gravityComp = Vector3.zero;

            OnLanded.Invoke(Velocity);
        }
        
        private void HandleCeilingAppeared() {
            if (IsGrounded || !_isGravityEnabled) return;

            _gravityComp.y = Mathf.Min(0f, _gravityComp.y);
            _targetInertia.y = Mathf.Min(0f, _targetInertia.y);
        }
        
        private void HandleColliderHit() {
            if (IsGrounded || !_isGravityEnabled) return;

            _inertiaComp = Vector3.ProjectOnPlane(_inertiaComp, _hitDetector.CollisionInfo.lastNormal);
            _targetInertia = _inertiaComp;
        }
        
        private void UpdateGrounded(float dt) {
            UpdateInertia(_massSettings.groundInertialFactor * dt);
        }
        
        private void UpdateInAir(float dt) {
            UpdateInertia(_massSettings.airInertialFactor * dt);
            UpdateGravity(dt);
        }

        private void UpdateInertia(float factor) {
            if (!_isGravityEnabled) return;

            _targetInertia = Vector3.Lerp(_targetInertia, Vector3.zero, factor);
            _inertiaComp = _targetInertia.RotateFromTo(Vector3.up, _groundDetector.CollisionInfo.lastNormal);
        }

        private void UpdateGravity(float dt) {
            if (!_isGravityEnabled) return;

            _gravityComp += _gravityDirection * (_massSettings.gravityForce * dt);
        }

        private Vector3 GetVelocity() {
            return _inertiaComp + _gravityComp + _forceComp;
        }

        // ---------------- ---------------- Debug ---------------- ----------------
        
#if UNITY_EDITOR
        [Header("Debug")] 
        [SerializeField] private bool _debugDrawGravity;
        [SerializeField] private bool _debugDrawInertia;
        [SerializeField] private bool _debugDrawForce;
        [SerializeField] private bool _debugDrawMotion;
        
        private void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            var pos = transform.position;
            if (_debugDrawInertia) DbgRay.Create().From(pos).Dir(_inertiaComp).Color(Color.yellow).Arrow(0.3f).Draw();
            if (_debugDrawGravity) DbgRay.Create().From(pos).Dir(_gravityComp).Color(Color.blue).Arrow(0.3f).Draw();
            if (_debugDrawForce) DbgRay.Create().From(pos).Dir(_forceComp).Color(Color.red).Arrow(0.3f).Draw();
            if (_debugDrawMotion) DbgRay.Create().From(pos).Dir(Velocity).Color(Color.white).Arrow(0.3f).Draw();
        }
#endif
        
    }

 }
