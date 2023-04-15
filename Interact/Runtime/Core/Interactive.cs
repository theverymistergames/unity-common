using System;
using System.Collections.Generic;
using MisterGames.Interact.Strategy;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class Interactive : MonoBehaviour, IInteractive, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        [SerializeField] private InteractiveStrategy _strategy;

        public event Action<IInteractiveUser> OnDetectedByUser = delegate {  };
        public event Action<IInteractiveUser> OnLostByUser = delegate {  };

        public event Action<IInteractiveUser, Vector3> OnStartInteract = delegate {  };
        public event Action<IInteractiveUser> OnStopInteract = delegate {  };

        public IInteractiveUser User => _interactUser;
        public Vector3 Position => _transform.position;

        public bool IsInteracting => _interactUser != null &&
                                     ReferenceEquals(_interactUser.PossibleInteractive, this) &&
                                     _interactUser.IsInteracting;

        public bool IsDetected => _detectedUsers.Count > 0;

        private readonly List<IInteractiveUser> _detectedUsers = new List<IInteractiveUser>();
        private IInteractiveUser _interactUser;
        private Transform _transform;

        private void Awake() {
            _transform = transform;
        }

        private void OnEnable() {
            if (IsInteracting || IsDetected) TimeSources.Get(_timeSourceStage).Subscribe(this);
        }

        private void OnDisable() {
            TimeSources.Get(_timeSourceStage).Unsubscribe(this);
        }

        public void OnUpdate(float dt) {
            _strategy.UpdateInteractionState(_interactUser, this);

            for (int i = 0; i < _detectedUsers.Count; i++) {
                _strategy.UpdateInteractionState(_detectedUsers[i], this);
            }
        }

        public void DetectByUser(IInteractiveUser user) {
            if (!_detectedUsers.Contains(user)) {
                _detectedUsers.Add(user);
                OnDetectedByUser.Invoke(user);
            }

            TimeSources.Get(_timeSourceStage).Subscribe(this);
        }

        public void LoseByUser(IInteractiveUser user) {
            _detectedUsers.Remove(user);
            OnLostByUser.Invoke(user);

            if (!IsInteracting && !IsDetected) TimeSources.Get(_timeSourceStage).Unsubscribe(this);
        }

        public void StartInteractWithUser(IInteractiveUser user, Vector3 hitPoint) {
            if (IsInteracting) StopInteractWithUser(_interactUser);

            _interactUser = user;
            OnStartInteract.Invoke(_interactUser, hitPoint);

            TimeSources.Get(_timeSourceStage).Subscribe(this);
        }

        public void StopInteractWithUser(IInteractiveUser user) {
            if (!IsInteracting || _interactUser != user) return;

            OnStopInteract.Invoke(_interactUser);
            _interactUser = null;

            if (!IsInteracting && !IsDetected) TimeSources.Get(_timeSourceStage).Unsubscribe(this);
        }

        public override string ToString() {
            return $"{nameof(Interactive)}({name}, {(_interactUser == null ? $"no user" : $"user {_interactUser.GameObject.name}")})";
        }
    }

}
