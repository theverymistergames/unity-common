using System;
using MisterGames.Character.Core;
using MisterGames.Character.Capsule;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionMaxHeight : ITransition, IDependency {

        [Min(0f)] public float maxHeight;

        public bool IsMatched => CheckCondition();

        private ICharacterCapsulePipeline _capsule;
        private ITransitionCallback _callback;
        
        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _capsule = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterCapsulePipeline>();
        }

        public void Arm(ITransitionCallback callback) {
            _callback = callback;

            _capsule.OnHeightChange -= OnHeightChanged;
            _capsule.OnHeightChange += OnHeightChanged;
        }

        public void Disarm() {
            _capsule.OnHeightChange -= OnHeightChanged;

            _callback = null;
        }

        public void OnFired() { }

        private void OnHeightChanged(float arg1, float arg2) {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private bool CheckCondition() {
            return _capsule.Height <= maxHeight;
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionMaxHeight)}({maxHeight})";
        }
    }

}
