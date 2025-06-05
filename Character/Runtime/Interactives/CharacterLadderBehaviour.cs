using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Character.Interactives {
    
    public sealed class CharacterLadderBehaviour : MonoBehaviour, IActorComponent, IJumpOverride {

        [SerializeField] private Transform _transform;
        [SerializeField] private Vector3 _normalRotation;
        [SerializeField] private TriggerListenerForRigidbody _triggerListenerForRigidbody;
        [SerializeField] private LayerMask _layerMask;
        
        [Header("Jump")]
        [SerializeField] private LabelValue _jumpPriority;
        [SerializeField] private float _jumpImpulse = 3f;
        
        [Header("Actions")]
        [SerializeReference] [SubclassSelector] private IActorAction _onEnter;
        [SerializeReference] [SubclassSelector] private IActorAction _onExit;

        private CancellationToken _destroyToken;
        
        private IActor _actor;
        private Rigidbody _rigidbody;
        private CharacterViewPipeline _characterViewPipeline;
        private CharacterJumpPipeline _characterJumpPipeline;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            _destroyToken = destroyCancellationToken;
        }

        private void OnEnable() {
            _triggerListenerForRigidbody.TriggerEnter += TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit += TriggerExit;
            
            if (_rigidbody != null) _onExit?.Apply(_actor, _destroyToken).Forget();
        }

        private void OnDisable() 
        {
            _triggerListenerForRigidbody.TriggerEnter -= TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit -= TriggerExit;
            
            if (_rigidbody != null) _onExit?.Apply(_actor, _destroyToken).Forget();
        }

        private void TriggerEnter(Rigidbody rigidbody) {
            if (!_layerMask.Contains(rigidbody.gameObject.layer) ||
                rigidbody.GetComponent<IActor>() is not { } actor ||
                !actor.TryGetComponent(out _characterViewPipeline) ||
                !actor.TryGetComponent(out _characterJumpPipeline)) 
            {
                return;
            }
            
            _rigidbody = rigidbody;
            _characterJumpPipeline.StartOverride(this, _jumpPriority.GetValue());
            _onEnter?.Apply(_actor, _destroyToken).Forget();
        }
        
        private void TriggerExit(Rigidbody rigidbody) {
            if (rigidbody != _rigidbody) {
                return;
            }
            
            _characterJumpPipeline.StopOverride(this);
            _onExit?.Apply(_actor, _destroyToken).Forget();

            _rigidbody = null;
            _characterViewPipeline = null;
            _characterJumpPipeline = null;
        }

        bool IJumpOverride.OnJumpRequested(ref float impulseDelay) {
            return true;
        }

        bool IJumpOverride.OnJumpImpulseRequested(ref Vector3 impulse) {
            var forward = _characterViewPipeline.HeadRotation * Vector3.forward;
            impulse = forward * _jumpImpulse;
            
            return true;
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