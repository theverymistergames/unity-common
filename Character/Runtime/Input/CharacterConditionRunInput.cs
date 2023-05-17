using System;
using MisterGames.Character.Core;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.Input {

    [Serializable]
    public sealed class CharacterConditionRunInput : ITransition, IDependency {

        public Optional<bool> isRunInputActive;
        public Optional<bool> isRunInputPressed;
        public Optional<bool> isRunInputReleased;

        public bool IsMatched => CheckCondition();

        private ICharacterInputPipeline _input;
        private ITransitionCallback _callback;

        public void OnAddDependencies(IDependencyResolver resolver) {
            resolver.AddDependency<CharacterAccess>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _input = resolver
                .ResolveDependency<CharacterAccess>()
                .GetPipeline<ICharacterInputPipeline>();
        }

        public void Arm(ITransitionCallback callback) {
            _callback = callback;

            _input.RunPressed -= OnRunPressed;
            _input.RunPressed += OnRunPressed;

            _input.RunReleased -= OnRunReleased;
            _input.RunReleased += OnRunReleased;
        }

        public void Disarm() {
            _input.RunPressed -= OnRunPressed;
            _input.RunReleased -= OnRunReleased;

            _callback = null;
        }

        private void OnRunPressed() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private void OnRunReleased() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private bool CheckCondition() {
            return isRunInputActive.IsEmptyOrEquals(_input.IsRunPressed) &&
                   isRunInputPressed.IsEmptyOrEquals(_input.WasRunPressed) &&
                   isRunInputReleased.IsEmptyOrEquals(_input.WasRunReleased);
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionRunInput)}(" +
                   $"active {isRunInputActive}, " +
                   $"pressed {isRunInputPressed}, " +
                   $"released {isRunInputReleased})";
        }
    }

}
