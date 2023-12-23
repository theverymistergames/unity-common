using System;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionCrouchInput : ITransition, IDependency {

        public Optional<bool> isCrouchInputActive;
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

            if (isCrouchInputActive.HasValue) {
                _input.OnCrouchPressed -= OnCrouchPressed;
                _input.OnCrouchPressed += OnCrouchPressed;
            }

            if (isCrouchInputActive.HasValue) {
                _input.OnCrouchReleased -= OnCrouchReleased;
                _input.OnCrouchReleased += OnCrouchReleased;
            }

            if (isCrouchInputToggled.HasValue) {
                _input.OnCrouchToggled -= OnCrouchToggled;
                _input.OnCrouchToggled += OnCrouchToggled;
            }
        }

        public void Disarm() {
            if (isCrouchInputActive.HasValue) _input.OnCrouchPressed -= OnCrouchPressed;
            if (isCrouchInputActive.HasValue) _input.OnCrouchReleased -= OnCrouchReleased;
            if (isCrouchInputToggled.HasValue) _input.OnCrouchToggled -= OnCrouchToggled;

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
                   isCrouchInputToggled.IsEmptyOrEquals(_input.WasCrouchToggled);
        }
    }

}
