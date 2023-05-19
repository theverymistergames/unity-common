using System;
using MisterGames.Character.Core;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Height {

    [Serializable]
    public sealed class CharacterConditionMaxHeight : ITransition, IDependency {

        [Min(0f)] public float maxHeight;

        public bool IsMatched => CheckCondition();

        private ICharacterHeightPipeline _heightPipeline;
        private ITransitionCallback _callback;
        
        public void OnAddDependencies(IDependencyContainer container) {
            container.AddDependency<CharacterAccess>(this);
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _heightPipeline = resolver
                .ResolveDependency<CharacterAccess>()
                .GetPipeline<ICharacterHeightPipeline>();
        }

        public void Arm(ITransitionCallback callback) {
            _callback = callback;

            _heightPipeline.OnHeightChanged -= OnHeightChanged;
            _heightPipeline.OnHeightChanged += OnHeightChanged;
        }

        public void Disarm() {
            _heightPipeline.OnHeightChanged -= OnHeightChanged;

            _callback = null;
        }

        public void OnFired() { }

        private void OnHeightChanged(float arg1, float arg2) {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private bool CheckCondition() {
            return _heightPipeline.Height <= maxHeight;
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionMaxHeight)}({maxHeight})";
        }
    }

}
