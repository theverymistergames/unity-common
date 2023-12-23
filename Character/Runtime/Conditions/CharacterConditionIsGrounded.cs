using System;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Collisions.Core;
using MisterGames.Common.Conditions;
using MisterGames.Common.Dependencies;

namespace MisterGames.Character.Conditions {

    [Serializable]
    public sealed class CharacterConditionIsGrounded : ICharacterCondition, ITransition, IDependency {

        public bool isGrounded;

        public bool IsMatch(ICharacterAccess context) {
            var groundDetector = context.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;
            return groundDetector.CollisionInfo.hasContact == isGrounded;
        }

        public bool IsMatched => CheckCondition();

        private ICollisionDetector _groundDetector;
        private ITransitionCallback _callback;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _groundDetector = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterCollisionPipeline>()
                .GroundDetector;
        }

        public void Arm(ITransitionCallback callback) {
            _callback = callback;

            _groundDetector.OnContact -= OnContact;
            _groundDetector.OnContact += OnContact;

            _groundDetector.OnLostContact -= OnLostContact;
            _groundDetector.OnLostContact += OnLostContact;
        }
        
        public void Disarm() {
            _groundDetector.OnContact -= OnContact;
            _groundDetector.OnLostContact -= OnLostContact;

            _callback = null;
        }

        public void OnFired() { }

        private void OnContact() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private void OnLostContact() {
            if (IsMatched) _callback?.OnTransitionMatch(this);
        }

        private bool CheckCondition() {
            return isGrounded == _groundDetector.CollisionInfo.hasContact;
        }

        public override string ToString() {
            return $"{nameof(CharacterConditionIsGrounded)}(isGrounded {isGrounded})";
        }
    }

}
