using System.Threading;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Collisions.Core;
using MisterGames.Character.Actions;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Jump {

    public sealed class CharacterJumpLandReactionPipeline : CharacterPipelineBase, ICharacterJumpLandReactionPipeline {

        [SerializeField] private CharacterAccess _characterAccess;

        [EmbeddedInspector]
        [SerializeField] private CharacterActionAsset _jumpReaction;

        [EmbeddedInspector]
        [SerializeField] private CharacterActionAsset _landReaction;

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterJumpPipeline _jump;
        private ICollisionDetector _groundDetector;
        private CancellationTokenSource _destroyCts;

        private void Awake() {
            _destroyCts = new CancellationTokenSource();
            _jump = _characterAccess.GetPipeline<ICharacterJumpPipeline>();
            _groundDetector = _characterAccess.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;
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
            if (_jumpReaction != null) await _jumpReaction.Apply(_characterAccess, this, _destroyCts.Token);
        }

        private async void OnLanded() {
            if (_landReaction != null) await _landReaction.Apply(_characterAccess, this, _destroyCts.Token);
        }
    }

}
