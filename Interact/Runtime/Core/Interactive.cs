using System;
using MisterGames.Common.Routines;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class Interactive : MonoBehaviour, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private InteractStrategy _strategy;
        
        public event Action<InteractiveUser, Vector3> OnStartInteract = delegate {  };
        public event Action OnStopInteract = delegate {  };

        private Transform _transform;
        private int _hash;
        
        private Transform _userTransform;
        private InteractiveUser _user;
        private Vector3 _point;
        private bool _isInteracting;

        private void Awake() {
            _transform = transform;
            _hash = _transform.GetHashCode();
        }

        private void OnEnable() {
            _timeDomain.SubscribeUpdate(this);
        }

        private void OnDisable() {
            _timeDomain.UnsubscribeUpdate(this);
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_isInteracting) return;

            if (IsValidDistance() && IsValidViewDirection()) return;
            
            StopInteract();
            StopInteractWithUser();
        }

        internal void OnUserStartInteract(InteractiveUser user, Vector3 point) {
            if (_user != null || _user == user) return;
            _user = user;
            _point = point;
            _userTransform = _user.transform;
        }

        internal void OnInteractInputPressed() {
            if (_user == null || !_strategy.inputAction.IsPressed) return;
            
            if (_isInteracting) {
                OnInputPressedWhileIsInteracting();
                return;
            }

            if (!IsValidDistance() || !IsValidViewDirection()) {
                StopInteractWithUser();
                return;
            }
            
            OnInputPressedWhileIsNotInteracting();
        }
        
        internal void OnInteractInputReleased() {
            if (_user == null || _strategy.inputAction.IsPressed) return;

            if (_isInteracting) {
                OnInputReleasedWhileIsInteracting();
                return;
            }

            OnInputReleasedWhileIsNotInteracting();
        }

        private void StartInteract() {
            _isInteracting = true;
            _strategy.Apply(_user);
            OnStartInteract.Invoke(_user, _point);
        }

        private void StopInteract() {
            _isInteracting = false;
            OnStopInteract.Invoke();
            _strategy.Release(_user);
        }

        private void StopInteractWithUser() {
            _user.StopInteractWith(this);
            _user = null;
            _userTransform = null;
        }
        
        private void OnInputPressedWhileIsInteracting() {
            switch (_strategy.mode) {
                case InteractStrategy.Mode.Tap: 
                    break;
                    
                case InteractStrategy.Mode.WhilePressed: 
                    break;
                    
                case InteractStrategy.Mode.ClickOnOff:
                    StopInteract();
                    StopInteractWithUser();    
                    break;
            }
        }
        
        private void OnInputPressedWhileIsNotInteracting() {
            switch (_strategy.mode) {
                case InteractStrategy.Mode.Tap:
                    StartInteract();
                    StopInteract();
                    StopInteractWithUser();
                    break;
                    
                case InteractStrategy.Mode.WhilePressed: 
                    StartInteract();
                    break;
                    
                case InteractStrategy.Mode.ClickOnOff:
                    StartInteract();
                    break;
            }
        }
        
        private void OnInputReleasedWhileIsInteracting() {
            switch (_strategy.mode) {
                case InteractStrategy.Mode.Tap: 
                    break;
                    
                case InteractStrategy.Mode.WhilePressed: 
                    StopInteract();
                    StopInteractWithUser();
                    break;
                    
                case InteractStrategy.Mode.ClickOnOff:
                    break;
            }
        }
        
        private void OnInputReleasedWhileIsNotInteracting() {
            switch (_strategy.mode) {
                case InteractStrategy.Mode.Tap:
                    break;
                    
                case InteractStrategy.Mode.WhilePressed: 
                    break;
                    
                case InteractStrategy.Mode.ClickOnOff:
                    break;
            }
        }

        private bool IsValidViewDirection() {
            if (!_strategy.stopInteractWhenNotInView) return true;
            return _user.PerformRayCast(out var hit) && hit.transform.GetHashCode() == _hash;
        }

        private bool IsValidDistance() {
            if (!_strategy.stopInteractWhenExceededMaxDistance) return true;
            
            float distance = Vector3.Distance(_userTransform.position, _transform.position);
            return distance < _strategy.maxInteractDistance;
        }
    }

}
