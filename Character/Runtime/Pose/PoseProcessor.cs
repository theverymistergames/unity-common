using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Collisions;
using MisterGames.Character.Configs;
using MisterGames.Character.View;
using MisterGames.Common.Maths;
using MisterGames.Fsm.Core;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Pose {

    public class PoseProcessor : MonoBehaviour {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [SerializeField] private StateMachineRunner _poseFsm;
        [SerializeField] private CharacterGroundDetector _groundDetector;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private CameraController _cameraController;
        
        public event Action OnCrouch = delegate {  }; 
        public event Action OnStand = delegate {  };

        public bool IsCrouching { get; private set;  }

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);
        private CancellationTokenSource _changePoseCts;
        private PoseStateData _initialPoseData;
        private PoseStateData _prevPoseData;

        private void OnEnable() {
            _cameraController.RegisterInteractor(this);
            _poseFsm.OnEnterState += HandleStateChanged;
        }

        private void OnDisable() {
            _cameraController.UnregisterInteractor(this);
            _poseFsm.OnEnterState -= HandleStateChanged;

            _changePoseCts?.Cancel();
            _changePoseCts?.Dispose();

            SetInitialParameters(_initialPoseData);
        }

        private void Start() {
            SetInitialParameters(_poseFsm.Instance.CurrentState.data as PoseStateData);
        }
        
        private void SetInitialParameters(PoseStateData data) {
            if (data == null) return;

            _prevPoseData = data;
            _initialPoseData = data; 
            _characterController.height = data.colliderHeight;
            _groundDetector.Distance = data.colliderHeight / 2f - _characterController.radius;
        }

        private void HandleStateChanged(FsmState state) {
            if (state.data is PoseStateData data) {
                var lastTransition = _poseFsm.Instance.LastTransition;
                if (lastTransition.data is PoseStateTransitionData transitionData) {
                    SetPoseData(data, transitionData);
                }
            }
        }
        
        private void SetPoseData(PoseStateData poseData, PoseStateTransitionData transitionData) {
            float prevTargetHeight = _prevPoseData.colliderHeight;
            float targetHeight = poseData.colliderHeight;
            float currHeight = _characterController.height;

            float currentToTarget = Mathf.Abs(currHeight - targetHeight);
            float targetsDiff = Mathf.Abs(prevTargetHeight - targetHeight);
            
            if (targetsDiff.IsNearlyZero()) {
                ApplyHeight(currHeight, targetHeight);
                return;
            }
            
            _prevPoseData = poseData;

            _changePoseCts?.Cancel();
            _changePoseCts?.Dispose();
            _changePoseCts = new CancellationTokenSource();

            float duration = transitionData.duration * currentToTarget / targetsDiff;
            ChangeHeight(targetHeight, duration, transitionData.curve, _changePoseCts.Token).Forget();
            
            CheckCrouchStateChanged(poseData.isCrouchState);
        }

        private void CheckCrouchStateChanged(bool isCrouching) {
            bool wasCrouching = IsCrouching;
            IsCrouching = isCrouching;

            if (wasCrouching && !isCrouching) {
                OnStand.Invoke();
                return;
            }

            if (!wasCrouching && isCrouching) {
                OnCrouch.Invoke();
            }
        }

        private async UniTaskVoid ChangeHeight(float targetHeight, float duration, AnimationCurve curve, CancellationToken token) {
            float prevHeight = _characterController.height;
            float diffHeight = targetHeight - prevHeight;
            float tempHeight = prevHeight;

            float time = 0f;
            while (!token.IsCancellationRequested) {
                time += _timeSource.DeltaTime;
                float process = Mathf.Clamp01(duration.IsNearlyZero() ? 1f : time / duration);

                float height = prevHeight + curve.Evaluate(process) * diffHeight;
                ApplyHeight(tempHeight, height);
                tempHeight = height;

                if (process >= 1f) break;

                bool isCancelled = await UniTask.NextFrame(token).SuppressCancellationThrow();
                if (isCancelled) break;
            }
        }

        private void ApplyHeight(float current, float target) {
            float diffToInitialHeight = target - _initialPoseData.colliderHeight;
                    
            var diffToInitialHeightVector = diffToInitialHeight * Vector3.up;

            _characterController.height = target;
            _characterController.center = diffToInitialHeightVector / 2f;
            _cameraController.SetOffset(this, diffToInitialHeightVector);

            if (!_groundDetector.CollisionInfo.hasContact) {
                _characterController.Move(Vector3.up * (current - target));
            }
        }
    }

}
