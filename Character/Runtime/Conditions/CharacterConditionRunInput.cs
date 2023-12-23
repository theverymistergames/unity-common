﻿using System;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Common.Conditions;
using MisterGames.Common.Data;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionRunInput : ITransition, IDependency {

        public Optional<bool> isRunInputActive;

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

            _input.OnRunPressed -= OnRunPressed;
            _input.OnRunPressed += OnRunPressed;

            _input.OnRunReleased -= OnRunReleased;
            _input.OnRunReleased += OnRunReleased;
        }

        public void Disarm() {
            _input.OnRunPressed -= OnRunPressed;
            _input.OnRunReleased -= OnRunReleased;

            _callback = null;
        }

        private void OnRunPressed() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private void OnRunReleased() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private bool CheckCondition() {
            return isRunInputActive.IsEmptyOrEquals(_input.IsRunPressed);
        }
    }

}
