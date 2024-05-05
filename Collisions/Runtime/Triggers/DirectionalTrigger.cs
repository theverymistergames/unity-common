using System;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Triggers {

    public sealed class DirectionalTrigger : MonoBehaviour {

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private bool _useExplicitDirection;

        [VisibleIf(nameof(_useExplicitDirection))]
        [SerializeField] private Transform _explicitDirection;

        [Header("Actions")]
        [SerializeReference] [SubclassSelector] private IActorAction _actionForward;
        [SerializeReference] [SubclassSelector] private IActorAction _actionBackwards;

        public event Action<GameObject> OnTriggeredForward = delegate {  };
        public event Action<GameObject> OnTriggeredBackward = delegate {  };

        private Transform _transform;

        private bool _isTrackingCollider;
        private int _trackedTransformHash;
        private Vector3 _enterPoint;

        private void Awake() {
            _transform = transform;
        }
        
        private void OnTriggerEnter(Collider other) {
            if (!enabled) return;
            if (_isTrackingCollider || !_layerMask.Contains(other.gameObject.layer)) return;

            var t = other.transform;
            
            _trackedTransformHash = t.GetHashCode();
            _isTrackingCollider = true;
            _enterPoint = t.position;
        }

        private void OnTriggerExit(Collider other) {
            if (!enabled) return;

            var go = other.gameObject;
            if (!_isTrackingCollider || !_layerMask.Contains(go.layer)) return;

            var t = other.transform;
            if (t.GetHashCode() != _trackedTransformHash) return;

            _isTrackingCollider = false;

            var triggerForward = _useExplicitDirection ? _explicitDirection.forward : _transform.forward;
            bool isForward = Vector3.Dot(t.forward, triggerForward) >= 0f ||
                             Vector3.Dot(t.position - _enterPoint, triggerForward) >= 0f;
            
            if (isForward) OnTriggeredForward.Invoke(go);
            else OnTriggeredBackward.Invoke(go);

            if ((isForward ? _actionForward : _actionBackwards) is {} action && go.TryGetComponent(out IActor actor)) {
                action.Apply(actor, destroyCancellationToken).Forget();
            }
        }
    }
    
}
