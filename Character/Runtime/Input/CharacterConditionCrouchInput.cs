using System;
using MisterGames.Character.Core;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.Input {

    [Serializable]
    public sealed class CharacterConditionCrouchInput : ITransition, IDependency {

        public Optional<bool> isCrouchInputActive;
        public Optional<bool> isCrouchInputPressed;
        public Optional<bool> isCrouchInputReleased;
        public Optional<bool> isCrouchInputToggled;

        public bool IsMatched => CheckCondition();

        private ICharacterInputPipeline _input;
        private ITransitionCallback _callback;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _input = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterInputPipeline>();
        }

        public void Arm(ITransitionCallback callback) {
            _callback = callback;

            if (isCrouchInputActive.HasValue || isCrouchInputPressed.HasValue) {
                _input.CrouchPressed -= OnCrouchPressed;
                _input.CrouchPressed += OnCrouchPressed;
            }

            if (isCrouchInputActive.HasValue || isCrouchInputReleased.HasValue) {
                _input.CrouchReleased -= OnCrouchReleased;
                _input.CrouchReleased += OnCrouchReleased;
            }

            if (isCrouchInputToggled.HasValue) {
                _input.CrouchToggled -= OnCrouchToggled;
                _input.CrouchToggled += OnCrouchToggled;
            }
        }

        public void Disarm() {
            if (isCrouchInputActive.HasValue || isCrouchInputPressed.HasValue) _input.CrouchPressed -= OnCrouchPressed;
            if (isCrouchInputActive.HasValue || isCrouchInputReleased.HasValue) _input.CrouchReleased -= OnCrouchReleased;
            if (isCrouchInputToggled.HasValue) _input.CrouchToggled -= OnCrouchToggled;

            _callback = null;
        }

        public void OnFired() { }

        private void OnCrouchPressed() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private void OnCrouchReleased() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private void OnCrouchToggled() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private bool CheckCondition() {
            return isCrouchInputActive.IsEmptyOrEquals(_input.IsCrouchPressed) &&
                   isCrouchInputPressed.IsEmptyOrEquals(_input.WasCrouchPressed) &&
                   isCrouchInputReleased.IsEmptyOrEquals(_input.WasCrouchReleased) &&
                   isCrouchInputToggled.IsEmptyOrEquals(_input.WasCrouchToggled);
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionCrouchInput)}(" +
                   $"active {isCrouchInputActive}, " +
                   $"pressed {isCrouchInputPressed}, " +
                   $"released {isCrouchInputReleased}, " +
                   $"toggled {isCrouchInputToggled})";
        }
    }

}
