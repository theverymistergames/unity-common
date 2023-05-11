using MisterGames.Character.Core;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Character.Collisions {

    public sealed class CharacterCollisionPipeline : CharacterPipelineBase, ICharacterCollisionPipeline {

        [SerializeField] private CharacterController _characterController;
        [SerializeField] private CharacterControllerHitDetector _hitDetector;
        [SerializeField] private CharacterCeilingDetector _ceilingDetector;
        [SerializeField] private CharacterGroundDetector _groundDetector;

        public ICollisionDetector HitDetector => _hitDetector;
        public IRadiusCollisionDetector CeilingDetector => _ceilingDetector;
        public IRadiusCollisionDetector GroundDetector => _groundDetector;

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        public override void SetEnabled(bool isEnabled) {
            _characterController.enabled = isEnabled;
            _hitDetector.enabled = isEnabled;
            _ceilingDetector.enabled = isEnabled;
            _groundDetector.enabled = isEnabled;
        }
    }

}
