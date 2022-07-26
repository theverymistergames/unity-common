using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class InteractiveUser : MonoBehaviour {
        
        [Header("Input")]
        [SerializeField] private InputActionKey _input;
        
        [Header("Raycast Settings")]
        [SerializeField] private float _maxDistance = 3f;
        [SerializeField] private float _radius = 0.5f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        private readonly RaycastHit[] _hits = new RaycastHit[6];
        private Transform _transform;
        private Interactive _currentInteractive;

        private void Awake() {
            _transform = transform;
        }

        private void OnEnable() {
            _input.OnPress += OnInputPressed;
            _input.OnRelease += OnInputReleased;
        }

        private void OnDisable() {
            _input.OnPress -= OnInputPressed;
            _input.OnRelease -= OnInputReleased;
        }

        internal void StopInteractWith(Interactive interactive) {
            if (_currentInteractive == interactive) {
                _currentInteractive = null;
            }
        }

        internal bool PerformRayCast(out RaycastHit hit) {
            int hitCount = Physics.SphereCastNonAlloc(
                _transform.position,
                _radius, 
                _transform.forward, 
                _hits,
                _maxDistance,
                _layerMask,
                _triggerInteraction
            );

            if (hitCount <= 0) {
                hit = default;
                return false;
            }

            hit = _hits[0];
            float distance = hit.distance;

            for (int i = 1; i < hitCount; i++) {
                var nextHit = _hits[i];
                if (nextHit.distance < distance) hit = nextHit;
            }

            return true;
        }

        private void OnInputPressed() {
            if (_currentInteractive != null) {
                _currentInteractive.OnInteractInputPressed();
                return;
            }

            if (!PerformRayCast(out var hit)) return;
            
            _currentInteractive = hit.transform.GetComponent<Interactive>();
            if (_currentInteractive == null) return;
            
            _currentInteractive.OnUserStartInteract(this, hit.point);
            _currentInteractive.OnInteractInputPressed();
        }

        private void OnInputReleased() {
            if (_currentInteractive == null) return;
            _currentInteractive.OnInteractInputReleased();
        }

    }

}