using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Capsule;
using MisterGames.Character.Phys;
using MisterGames.Common.Async;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class CharacterMotionGraphPipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private ActorAction _action;

        private readonly BlockSet _blockSet = new();
        
        private IActor _actor;
        private CharacterPosePipeline _pose;
        private CharacterMotionPipeline _motionPipeline;
        private CharacterMotionRunPipeline _run;
        private CharacterGroundDetector _groundDetector;
        private CancellationTokenSource _enableCts;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
            _pose = actor.GetComponent<CharacterPosePipeline>();
            _run = actor.GetComponent<CharacterMotionRunPipeline>();
            _motionPipeline = actor.GetComponent<CharacterMotionPipeline>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
        }

        private void OnEnable() {
            _blockSet.OnUpdate += UpdateState;
            
            UpdateState();
        }

        private void OnDisable() {
            _blockSet.OnUpdate -= UpdateState;
            
            DisableGraph();
        }

        public void SetBlock(object source, bool blocked, CancellationToken cancellationToken = default) {
            _blockSet.SetBlock(source, blocked, cancellationToken);
        }

        private void UpdateState() {
            if (_blockSet.Count <= 0) EnableGraph();
            else DisableGraph();
            
            ApplyState();
        }

        private void OnStartContactGround() {
            ApplyState();
        }

        private void OnStopContactGround() {
            ApplyState();
        }

        private void OnPoseChanged(CharacterPose newPose, CharacterPose oldPose) {
            ApplyState();
        }

        private void OnRunPressed() {
            ApplyState();
        }

        private void EnableGraph() {
            if (_enableCts != null) return;
            
            AsyncExt.RecreateCts(ref _enableCts);
            
            _groundDetector.OnContact += OnStartContactGround;
            _groundDetector.OnLostContact += OnStopContactGround;
            _pose.OnPoseChanged += OnPoseChanged;
            _run.OnRunStateChanged += OnRunPressed;
            _motionPipeline.OnTeleport += OnTeleport;
        }

        private void DisableGraph() {
            if (_enableCts == null) return;
            
            AsyncExt.DisposeCts(ref _enableCts);
            
            _groundDetector.OnContact -= OnStartContactGround;
            _groundDetector.OnLostContact -= OnStopContactGround;
            _pose.OnPoseChanged -= OnPoseChanged;
            _run.OnRunStateChanged -= OnRunPressed;
            _motionPipeline.OnTeleport -= OnTeleport;
        }

        private void OnTeleport() {
            ApplyState();
        }

        private void ApplyState() {
            if (_enableCts == null) return; 
            
            _action.Apply(_actor, _enableCts.Token).Forget();
        }
    }

}
