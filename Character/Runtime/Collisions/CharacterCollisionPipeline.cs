using MisterGames.Actors;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public sealed class CharacterCollisionPipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private CharacterControllerHitDetector _hitDetector;
        [SerializeField] private CharacterCeilingDetector _ceilingDetector;
        [SerializeField] private CharacterGroundDetector _groundDetector;

        public ICollisionDetector HitDetector => _hitDetector;
        public IRadiusCollisionDetector CeilingDetector => _ceilingDetector;
        public IRadiusCollisionDetector GroundDetector => _groundDetector;

        private CharacterController _characterController;
        
        public void OnAwake(IActor actor) {
            _characterController = actor.GetComponent<CharacterController>();
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void SetEnabled(bool isEnabled) {
            _characterController.enabled = isEnabled;
            _hitDetector.enabled = isEnabled;
            _ceilingDetector.enabled = isEnabled;
            _groundDetector.enabled = isEnabled;
        }
    }

}
