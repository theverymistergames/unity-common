using System;
using MisterGames.Common.Maths;
using MisterGames.Common.Routines;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class Interactive : MonoBehaviour, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private InteractStrategy _strategy;

        public event Action<InteractiveUser> OnStartInteractBy = delegate {  };
        public event Action OnStopInteract = delegate {  };

        private Transform _transform;

        private InteractiveUser _lastDetectedUser;
        private InteractiveUser _interactionUser;
        private Transform _interactionUserTransform;
        private IInteractiveMode _strategyMode;

        private bool _isInteracting;
        private bool _isDetectedByInteractionUser;
        private float _maxInteractionSqrDistance;

        private void Awake() {
            _transform = transform;
            _maxInteractionSqrDistance = _strategy.maxInteractDistance * _strategy.maxInteractDistance;
            _strategyMode = _strategy.mode.Build();
        }

        private void OnDisable() {
            _timeDomain.UnsubscribeUpdate(this);
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_isInteracting) return;
            if (IsValidDistance() && IsValidViewDirection()) return;
            
            StopInteractByUser(_interactionUser);
        }

        public void OnDetectedByUser(InteractiveUser user) {
            if (user == null) return;

            _lastDetectedUser = user;
            _isDetectedByInteractionUser = _isInteracting && _interactionUser.IsDetectedTarget(this);

            SubscribeOnInputAction();
        }

        public void OnLostByUser(InteractiveUser user) {
            _isDetectedByInteractionUser = _isInteracting && _interactionUser.IsDetectedTarget(this);
            _lastDetectedUser = null;

            if (_isInteracting) return;

            UnsubscribeFromInputAction();
        }

        public void StartInteractByUser(InteractiveUser user) {
            if (_isInteracting || user == null) return;

            SetInteractionUser(user);

            if (!IsValidDistance() || !IsValidViewDirection()) {
                ResetInteractionUser();
                return;
            }

            _strategy.filter.Apply();
            _timeDomain.SubscribeUpdate(this);

            OnStartInteractBy.Invoke(_interactionUser);
        }

        public void StopInteractByUser(InteractiveUser user) {
            if (!_isInteracting || user != _interactionUser) return;

            _timeDomain.UnsubscribeUpdate(this);
            _strategy.filter.Release();
            ResetInteractionUser();

            OnStopInteract.Invoke();
        }

        private void OnInteractInputPressed() {
            if (_isInteracting) {
                _strategyMode.OnInputPressedWhileIsInteracting(_interactionUser, this);
                return;
            }

            if (_lastDetectedUser == null) return;
            _strategyMode.OnInputPressedWhileIsNotInteracting(_lastDetectedUser, this);
        }

        private void OnInteractInputReleased() {
            if (_isInteracting) {
                _strategyMode.OnInputReleasedWhileIsInteracting(_interactionUser, this);
            }
        }

        private void SubscribeOnInputAction() {
            _strategy.inputAction.OnPress -= OnInteractInputPressed;
            _strategy.inputAction.OnRelease -= OnInteractInputReleased;

            _strategy.inputAction.OnPress += OnInteractInputPressed;
            _strategy.inputAction.OnRelease += OnInteractInputReleased;
        }

        private void UnsubscribeFromInputAction() {
            _strategy.inputAction.OnPress -= OnInteractInputPressed;
            _strategy.inputAction.OnRelease -= OnInteractInputReleased;
        }

        private void SetInteractionUser(InteractiveUser user) {
            _interactionUser = user;
            _interactionUserTransform = _interactionUser.transform;
            _isInteracting = true;
            _isDetectedByInteractionUser = _interactionUser.IsDetectedTarget(this);
        }

        private void ResetInteractionUser() {
            _interactionUser = null;
            _interactionUserTransform = null;
            _isInteracting = false;
            _isDetectedByInteractionUser = false;
        }

        private bool IsValidViewDirection() {
            return !_strategy.stopInteractWhenNotInView || !_isDetectedByInteractionUser;
        }

        private bool IsValidDistance() {
            if (!_strategy.stopInteractWhenExceededMaxDistance) return true;
            
            float sqrDistance = _transform.position.SqrDistanceTo(_interactionUserTransform.position);
            return sqrDistance < _maxInteractionSqrDistance;
        }
    }

}
