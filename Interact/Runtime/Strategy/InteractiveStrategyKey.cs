using System;
using MisterGames.Common.Data;
using MisterGames.Input.Actions;
using UnityEngine;

namespace MisterGames.Interact.Core {

    [Serializable]
    public sealed class InteractiveStrategyKey : IInteractiveStrategy {

        [Header("Input Settings")]
        [SerializeField] private InputActionKeyEvent _inputStart;
        [SerializeField] private InputActionKeyEvent _inputStop;

        [Header("Conditions")]
        [SerializeField] private bool _stopInteractWhenNotInView;
        [SerializeField] private Optional<float> _maxInteractDistance;

        public void UpdateInteractionState(IInteractiveUser user, IInteractive interactive) {
            if (user == null || interactive == null) return;

            if (_inputStart.WasFired) TryStartInteract(user, interactive);
            if (_inputStop.WasFired || !CanContinueInteract(user, interactive)) TryStopInteract(user, interactive);
        }

        private void TryStartInteract(IInteractiveUser user, IInteractive interactive) {
            if (user == null ||
                interactive == null ||
                user.PossibleInteractive != interactive ||
                user.IsInteracting ||
                !CanContinueInteract(user, interactive)) return;

            user.StartInteract();
        }

        private void TryStopInteract(IInteractiveUser user, IInteractive interactive) {
            if (user == null ||
                interactive == null ||
                user.PossibleInteractive != interactive ||
                !user.IsInteracting) return;

            user.StopInteract();
        }

        private void TryToggleInteractionState(IInteractiveUser user, IInteractive interactive) {
            if (user == null ||
                interactive == null ||
                user.PossibleInteractive != interactive) return;

            if (user.IsInteracting) {
                user.StopInteract();
                return;
            }

            if (CanContinueInteract(user, interactive)) user.StartInteract();
        }

        private bool CanContinueInteract(IInteractiveUser user, IInteractive interactive) {
            if (_stopInteractWhenNotInView && user.PossibleInteractive != interactive) return false;

            if (_maxInteractDistance.HasValue) {
                float sqrDistance = Vector3.SqrMagnitude(user.Position - interactive.Position);
                if (sqrDistance > _maxInteractDistance.Value * _maxInteractDistance.Value) return false;
            }

            return true;
        }
    }

}
