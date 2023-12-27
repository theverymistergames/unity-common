using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Capsule;
using MisterGames.Character.Collisions;
using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public class CharacterMotionGraphPipeline : CharacterPipelineBase, ICharacterPipeline {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private CharacterActionAsset _action;

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        private ICharacterPosePipeline _pose;
        private ICharacterInputPipeline _input;
        private ICollisionDetector _groundDetector;
        private CancellationTokenSource _enableCts;

        private void Awake() {
            _pose = _characterAccess.GetPipeline<ICharacterPosePipeline>();
            _input = _characterAccess.GetPipeline<ICharacterInputPipeline>();
            _groundDetector = _characterAccess.GetPipeline<ICharacterCollisionPipeline>().GroundDetector;
        }

        private void OnEnable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = new CancellationTokenSource();

            _groundDetector.OnContact -= OnStartContactGround;
            _groundDetector.OnContact += OnStartContactGround;

            _groundDetector.OnLostContact -= OnStopContactGround;
            _groundDetector.OnLostContact += OnStopContactGround;

            _pose.OnPoseChanged -= OnPoseChanged;
            _pose.OnPoseChanged += OnPoseChanged;

            _input.OnRunPressed -= OnRunPressed;
            _input.OnRunPressed += OnRunPressed;

            _input.OnRunReleased -= OnRunReleased;
            _input.OnRunReleased += OnRunReleased;
        }

        private void OnDisable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = null;

            _groundDetector.OnContact -= OnStartContactGround;
            _groundDetector.OnLostContact -= OnStopContactGround;

            _pose.OnPoseChanged -= OnPoseChanged;

            _input.OnRunPressed -= OnRunPressed;
            _input.OnRunReleased -= OnRunReleased;
        }

        private void OnStartContactGround() {
            ApplyActionAndForget(_action);
        }

        private void OnStopContactGround() {
            ApplyActionAndForget(_action);
        }

        private void OnPoseChanged(CharacterPose newPose, CharacterPose oldPose) {
            ApplyActionAndForget(_action);
        }

        private void OnRunPressed() {
            ApplyActionAndForget(_action);
        }

        private void OnRunReleased() {
            ApplyActionAndForget(_action);
        }

        private void ApplyActionAndForget(ICharacterAction action) {
            action.Apply(_characterAccess, _enableCts.Token).Forget();
        }
    }

}
