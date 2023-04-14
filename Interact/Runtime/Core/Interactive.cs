using System;
using MisterGames.Tick.Core;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class Interactive : MonoBehaviour, IInteractive, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        [SerializeField] private InteractiveStrategy _strategy;

        public event Action<IInteractiveUser> OnDetectedByUser = delegate {  };
        public event Action<IInteractiveUser> OnLostByUser = delegate {  };

        public event Action<IInteractiveUser, Vector3> OnStartInteract = delegate {  };
        public event Action<IInteractiveUser> OnStopInteract = delegate {  };

        public IInteractiveUser User => _currentUser;
        public Vector3 Position => _transform.position;

        public bool IsInteracting => _currentUser != null && ReferenceEquals(_currentUser.PossibleInteractive, this);
        public bool IsDetected => _detectedByUsersCount > 0;

        private Transform _transform;
        private IInteractiveUser _currentUser;
        private int _detectedByUsersCount;

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
            if (IsInteracting) _strategy.UpdateInteractionState(_currentUser, this);
        }

        public void DetectByUser(IInteractiveUser user) {
            _detectedByUsersCount++;
            OnDetectedByUser.Invoke(user);

            TimeSources.Get(_timeSourceStage).Subscribe(this);
        }

        public void LoseByUser(IInteractiveUser user) {
            _detectedByUsersCount = math.max(0, _detectedByUsersCount - 1);
            OnLostByUser.Invoke(user);

            if (!IsInteracting && !IsDetected) TimeSources.Get(_timeSourceStage).Unsubscribe(this);
        }

        public void StartInteractWithUser(IInteractiveUser user, Vector3 hitPoint) {
            if (IsInteracting) StopInteractWithUser(_currentUser);

            _currentUser = user;
            OnStartInteract.Invoke(_currentUser, hitPoint);

            TimeSources.Get(_timeSourceStage).Subscribe(this);
        }

        public void StopInteractWithUser(IInteractiveUser user) {
            if (!IsInteracting || _currentUser != user) return;

            OnStopInteract.Invoke(_currentUser);
            _currentUser = null;

            if (!IsInteracting && !IsDetected) TimeSources.Get(_timeSourceStage).Unsubscribe(this);
        }

        public override string ToString() {
            return $"{nameof(Interactive)}({name}, user = {_currentUser})";
        }
    }

}
