using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Collisions.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Jump {

    public sealed class CharacterJumpLandReactionPipeline : CharacterPipelineBase, ICharacterJumpLandReactionPipeline {

        [SerializeField] private CharacterAccess _characterAccess;

        [EmbeddedInspector]
        [SerializeField] private AsyncActionAsset _jumpReaction;

        [EmbeddedInspector]
        [SerializeField] private AsyncActionAsset _landReaction;

        [RuntimeDependency(typeof(ICharacterAccess))]
        [FetchDependencies(nameof(_jumpReaction))]
        [FetchDependencies(nameof(_landReaction))]
        [SerializeField] private DependencyResolver _dependencies;

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterJumpPipeline _jump;
        private ICollisionDetector _groundDetector;
        private CancellationTokenSource _destroyCts;

        private void Awake() {
            _destroyCts = new CancellationTokenSource();
            _jump = _characterAccess.GetPipeline<ICharacterJumpPipeline>();
            _groundDetector = _characterAccess.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;

            _dependencies.SetValue<ICharacterAccess>(_characterAccess);
            _dependencies.Resolve(_jumpReaction);
            _dependencies.Resolve(_landReaction, additive: true);
        }

        private void OnDestroy() {
            _destroyCts.Cancel();
            _destroyCts.Dispose();
        }

        private void OnEnable() {
            _jump.OnJump -= OnJump;
            _jump.OnJump += OnJump;

            _groundDetector.OnContact -= OnLanded;
            _groundDetector.OnContact += OnLanded;
        }

        private void OnDisable() {
            _jump.OnJump -= OnJump;
            _groundDetector.OnContact -= OnLanded;
        }

        private async void OnJump(Vector3 vector3) {
            await _jumpReaction.TryApply(this, _destroyCts.Token);
        }

        private async void OnLanded() {
            await _landReaction.TryApply(this, _destroyCts.Token);
        }
    }

}
