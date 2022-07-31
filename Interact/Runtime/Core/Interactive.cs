using System;
using MisterGames.Common.Maths;
using MisterGames.Common.Routines;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class Interactive : MonoBehaviour, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private InteractStrategy _strategy;

        public event Action<InteractiveUser> OnStartInteract = delegate {  };
        public event Action OnStopInteract = delegate {  };

        public InteractStrategy Strategy => _strategy;
        public bool IsInteracting { get; private set; }

        private Transform _transform;

        private InteractiveUser _lastDetectedUser;
        private InteractiveUser _interactiveUser;
        private Transform _interactionUserTransform;
        private IInteractiveMode _strategyMode;

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
            if (!IsInteracting) return;
            if (IsValidDistance() && IsValidViewDirection()) return;
            
            StopInteractByUser(_interactiveUser);
        }

        public void OnDetectedByUser(InteractiveUser user) {
            if (user == null) return;

            _lastDetectedUser = user;
            _isDetectedByInteractionUser = IsInteracting && _interactiveUser.IsDetectedTarget(this);

            SubscribeOnInputAction();
        }

        public void OnLostByUser(InteractiveUser user) {
            _isDetectedByInteractionUser = IsInteracting && _interactiveUser.IsDetectedTarget(this);
            _lastDetectedUser = null;

            if (IsInteracting) return;

            UnsubscribeFromInputAction();
        }

        public void StartInteractByUser(InteractiveUser user) {
            if (IsInteracting || user == null) return;

            SetInteractionUser(user);

            if (!IsValidDistance() || !IsValidViewDirection()) {
                ResetInteractionUser();
                return;
            }

            _strategy.filter.Apply();
            _timeDomain.SubscribeUpdate(this);

            OnStartInteract.Invoke(_interactiveUser);
        }

        public void StopInteractByUser(InteractiveUser user) {
            if (!IsInteracting || user != _interactiveUser) return;

            _timeDomain.UnsubscribeUpdate(this);
            _strategy.filter.Release();
            ResetInteractionUser();

            OnStopInteract.Invoke();
        }

        private void OnInteractInputPressed() {
            if (IsInteracting) {
                _strategyMode.OnInputPressedWhileIsInteracting(_interactiveUser, this);
                return;
            }

            if (_lastDetectedUser == null) return;
            _strategyMode.OnInputPressedWhileIsNotInteracting(_lastDetectedUser, this);
        }

        private void OnInteractInputReleased() {
            if (IsInteracting) {
                _strategyMode.OnInputReleasedWhileIsInteracting(_interactiveUser, this);
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
            _interactiveUser = user;
            _interactionUserTransform = _interactiveUser.transform;
            IsInteracting = true;
            _isDetectedByInteractionUser = _interactiveUser.IsDetectedTarget(this);
        }

        private void ResetInteractionUser() {
            _interactiveUser = null;
            _interactionUserTransform = null;
            IsInteracting = false;
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

        public override string ToString() {
            return $"{nameof(Interactive)}(" +
                   $"{name}" +
                   $", user = {(_interactiveUser == null ? "null" : $"{_interactiveUser.name}")}" +
                   ")";
        }
    }

}
