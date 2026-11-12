using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Character.Phys;
using MisterGames.Character.View;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Interactives {
    
    public sealed class CharacterLadderBehaviour : MonoBehaviour, IActorComponent, IJumpOverride, IMotionProcessor {

        [Header("Ladder")]
        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _normalRotation;
        [SerializeField] private TriggerListenerForRigidbody _triggerListenerForRigidbody;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private Collider[] _collidersZone;
        
        [Header("Motion")]
        [SerializeField] private int _orientPriority = 2;
        [SerializeField] [Range(0f, 180f)] private float _outerNormalAngle = 90f;

        [Header("Jump")]
        [SerializeField] private LabelValue _jumpPriority;
        [SerializeField] private float _jumpImpulse = 3f;
        [SerializeField] [Min(0f)] private float _jumpIgnoreColliderDelay = 0.7f;
        
        [Header("Actions")]
        [SerializeReference] [SubclassSelector] private IActorAction _onEnter;
        [SerializeReference] [SubclassSelector] private IActorAction _onExit;

        private CancellationToken _destroyToken;
        
        private IActor _ladderActor;
        
        private IActor _clientActor;
        private Rigidbody _clientRb;
        private CharacterMotionPipeline _motionPipeline;
        private CharacterViewPipeline _viewPipeline;
        private CharacterJumpPipeline _jumpPipeline;
        private CharacterGroundDetector _groundDetector;
        private readonly Dictionary<EntityId, byte> _colliderIgnoreIdMap = new();

        void IActorComponent.OnAwake(IActor actor) {
            _ladderActor = actor;
        }

        private void Awake() {
            _destroyToken = destroyCancellationToken;
        }

        private void OnEnable() {
            _triggerListenerForRigidbody.TriggerEnter += TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit += TriggerExit;
            
            if (_clientRb != null) _onExit?.Apply(_ladderActor, _destroyToken).Forget();
        }

        private void OnDisable() {
            _triggerListenerForRigidbody.TriggerEnter -= TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit -= TriggerExit;
            
            if (_clientRb != null) _onExit?.Apply(_ladderActor, _destroyToken).Forget();
        }

        private void TriggerEnter(Rigidbody rigidbody) {
            if (!_layerMask.Contains(rigidbody.gameObject.layer) ||
                rigidbody.GetComponent<IActor>() is not { } actor ||
                !actor.TryGetComponent(out _viewPipeline) ||
                !actor.TryGetComponent(out _jumpPipeline) || 
                !actor.TryGetComponent(out _motionPipeline) || 
                !actor.TryGetComponent(out _groundDetector)) 
            {
                return;
            }

            _clientActor = actor;
            _clientRb = rigidbody;
            _jumpPipeline.StartOverride(this, _jumpPriority.GetValue());
            _motionPipeline.AddProcessor(this);
            
            _onEnter?.Apply(_ladderActor, _destroyToken).Forget();
        }
        
        private void TriggerExit(Rigidbody rigidbody) {
            if (rigidbody != _clientRb) {
                return;
            }
            
            _jumpPipeline.StopOverride(this);
            _motionPipeline.RemoveProcessor(this);
            _onExit?.Apply(_ladderActor, _destroyToken).Forget();

            _clientActor = null;
            _clientRb = null;
            _viewPipeline = null;
            _jumpPipeline = null;
            _motionPipeline = null;
            _groundDetector = null;
        }

        bool IMotionProcessor.ProcessOrientation(ref Quaternion orientation, out int priority) {
            var normal = GetNormal(_clientRb.position);
            var headDir = _viewPipeline.HeadDirection;
            float angle = Vector3.Angle(normal, headDir);

            priority = _orientPriority;
            
            if (angle >= _outerNormalAngle || _motionPipeline.Input.y >= 0f) {
                return false;
            }

            // Input down: move straight down or on ground
            var up = _viewPipeline.BodyUp;
            orientation = _groundDetector.HasContact
                ? Quaternion.LookRotation(Vector3.ProjectOnPlane(headDir, up), up)
                : Quaternion.LookRotation(up, Vector3.ProjectOnPlane(headDir, up));
            return true;
        }

        bool IJumpOverride.OnJumpRequested(ref float impulseDelay) {
            return true;
        }

        bool IJumpOverride.OnJumpImpulseRequested(ref Vector3 impulse) {
            var forward = _viewPipeline.HeadRotation * Vector3.forward;
            impulse = forward * _jumpImpulse;

            IgnoreColliders(_clientActor, _jumpIgnoreColliderDelay, _destroyToken).Forget();
            
            return true;
        }

        private async UniTask IgnoreColliders(IActor withActor, float delay, CancellationToken ct) {
            if (withActor?.GameObject == null) return;
            
            SetIgnoreColliders(withActor, true);

            byte id = GetColliderIgnoreId(withActor);
            
            await AsyncExt.Delay(delay, ct);
            if (ct.IsCancellationRequested || id != GetColliderIgnoreId(withActor)) return;
            
            SetIgnoreColliders(withActor, false);
        }

        private void SetIgnoreColliders(IActor withActor, bool ignore) {
            if (withActor?.GameObject == null) return;
            
            IncrementColliderIgnoreId(withActor);
            
            var colliders = withActor.GetComponents<Collider>();
            foreach (var c in colliders) {
                for (int i = 0; i < _collidersZone.Length; i++) {
                    var collider = _collidersZone[i];
                    Physics.IgnoreCollision(collider, c, ignore);
                }
            }
        }

        private byte GetColliderIgnoreId(IActor withActor) {
            var entId = withActor.GameObject.GetEntityId();
            return _colliderIgnoreIdMap.GetValueOrDefault(entId);
        }

        private void IncrementColliderIgnoreId(IActor withActor) {
            var entId = withActor.GameObject.GetEntityId();
            byte id = _colliderIgnoreIdMap.GetValueOrDefault(entId); 
            _colliderIgnoreIdMap[entId] = id.IncrementUncheckedRef();
        }
        
        private Vector3 GetNormal(Vector3 position) {
            _transform.GetPositionAndRotation(out var pos, out var rot);
            var normal = rot * Vector3.forward;

            return Quaternion.Euler(_normalRotation) * (normal * Mathf.Sign(Vector3.Dot(position - pos, normal)));
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void Reset() {
            _transform = transform;
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo || _transform == null) return;

            var p = _transform.position;
            DebugExt.DrawRay(p, GetNormal(p + _transform.rotation * Vector3.forward), Color.cyan, gizmo: true);
            DebugExt.DrawSphere(p, 0.05f, Color.cyan, gizmo: true);
        }
#endif
    }
    
}