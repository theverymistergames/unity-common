using MisterGames.Actors;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public sealed class CharacterCollisionPipeline : MonoBehaviour, IActorComponent {
        
        public IRadiusCollisionDetector CeilingDetector => _ceilingDetector;
        public IRadiusCollisionDetector GroundDetector => _groundDetector;

        private CharacterCeilingDetector _ceilingDetector;
        private CharacterGroundDetector _groundDetector;
        private CapsuleCollider _collider;

        void IActorComponent.OnAwake(IActor actor) {
            _ceilingDetector = actor.GetComponent<CharacterCeilingDetector>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _collider = actor.GetComponent<CapsuleCollider>();
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void SetEnabled(bool isEnabled) {
            _ceilingDetector.enabled = isEnabled;
            _groundDetector.enabled = isEnabled;
            _collider.isTrigger = !isEnabled;
        }
    }

}
