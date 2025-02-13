using System.Threading;
using MisterGames.Actors;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Phys {

    public sealed class CharacterCollisionPipeline : MonoBehaviour, IActorComponent {

        private readonly BlockSet _blockSet = new();
        
        private CharacterCeilingDetector _ceilingDetector;
        private CharacterGroundDetector _groundDetector;
        private CapsuleCollider _collider;

        void IActorComponent.OnAwake(IActor actor) {
            _ceilingDetector = actor.GetComponent<CharacterCeilingDetector>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
            _collider = actor.GetComponent<CapsuleCollider>();
        }

        private void OnEnable() {
            _blockSet.OnUpdate += OnBlocksUpdated;
            OnBlocksUpdated();
        }

        private void OnDisable() {
            _blockSet.OnUpdate -= OnBlocksUpdated;
            SetEnabled(false);
        }

        public void Block(object source, bool blocked, CancellationToken cancellationToken = default) {
            _blockSet.SetBlock(source, blocked, cancellationToken);
        }

        private void OnBlocksUpdated() {
            SetEnabled(_blockSet.Count == 0);
        }

        private void SetEnabled(bool isEnabled) {
            _ceilingDetector.enabled = isEnabled;
            _groundDetector.enabled = isEnabled;
            _collider.isTrigger = !isEnabled;
        }
    }

}
