using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Collisions;
using MisterGames.Collisions.Core;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Motion {

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
            AsyncExt.RecreateCts(ref _enableCts);

            _jump.OnJumpRequest += OnJumpRequest;
            _groundDetector.OnContact += OnLanded;
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _jump.OnJumpRequest -= OnJumpRequest;
            _groundDetector.OnContact -= OnLanded;
        }

        private void OnJumpRequest() {
            ApplyAction(_jumpReaction, _enableCts.Token).Forget();
        }

        private void OnLanded() {
            ApplyAction(_landReaction, _enableCts.Token).Forget();
        }

        private UniTask ApplyAction(IActorAction action, CancellationToken cancellationToken) {
            return action?.Apply(_actor, cancellationToken) ?? UniTask.CompletedTask;
        }
    }

}
