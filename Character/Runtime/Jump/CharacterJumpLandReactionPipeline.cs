using System.Threading;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Collisions;
using MisterGames.Collisions.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Jump {

    public sealed class CharacterJumpLandReactionPipeline : MonoBehaviour, IActorComponent {

        [EmbeddedInspector]
        [SerializeField] private ActorAction _jumpReaction;

        [EmbeddedInspector]
        [SerializeField] private ActorAction _landReaction;
        
        private IActor _actor;
        private CharacterJumpPipeline _jump;
        private ICollisionDetector _groundDetector;
        private CancellationTokenSource _enableCts;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
            _jump = actor.GetComponent<CharacterJumpPipeline>();
            _groundDetector = actor.GetComponent<CharacterCollisionPipeline>().GroundDetector;
        }

        private void OnEnable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = new CancellationTokenSource();

            _jump.OnJump -= OnJump;
            _jump.OnJump += OnJump;

            _groundDetector.OnContact -= OnLanded;
            _groundDetector.OnContact += OnLanded;
        }

        private void OnDisable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = null;

            _jump.OnJump -= OnJump;
            _groundDetector.OnContact -= OnLanded;
        }

        private async void OnJump(Vector3 vector3) {
            if (_jumpReaction != null) await _jumpReaction.Apply(_actor, _enableCts.Token);
        }

        private async void OnLanded() {
            if (_landReaction != null) await _landReaction.Apply(_actor, _enableCts.Token);
        }
    }

}
