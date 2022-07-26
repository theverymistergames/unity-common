using System;
using MisterGames.Character.Motion;
using MisterGames.Character.Phys;
using MisterGames.Character.Pose;
using UnityEngine;

namespace MisterGames.Character.Systems {

    public class CharacterMotionEvents : MonoBehaviour {

        [SerializeField] private JumpProcessor _jumpProcessor;
        [SerializeField] private CharacterAdapter _adapter;
        [SerializeField] private MassProcessor _massProcessor;
        [SerializeField] private PoseProcessor _poseProcessor;
        [SerializeField] private RunProcessor _runProcessor;
        [SerializeField] private MotionInputProcessor _motionInputProcessor;

        public event Action OnCrouch = delegate {  };
        public event Action OnStand = delegate {  };
        
        public event Action OnStartRun = delegate {  };
        public event Action OnStopRun = delegate {  };
        
        public event Action OnFell = delegate {  };
        public event Action<Vector3> OnLanded = delegate {  };
        public event Action<Vector3> OnJump = delegate {  };
        
        public event Action OnStartMoving = delegate {  };
        public event Action OnStopMoving = delegate {  };

        public Vector3 Velocity => _adapter.Velocity;
        public bool IsGrounded => _massProcessor.IsGrounded;
        public bool IsCrouching => _poseProcessor.IsCrouching;
        public bool IsRunning => _runProcessor.IsRunning;
        
        private void OnEnable() {
            _massProcessor.OnFell += HandleFell;
            _massProcessor.OnLanded += HandleLanded;
            
            _jumpProcessor.OnJump += HandleJump;
            
            _poseProcessor.OnCrouch += HandleCrouch;
            _poseProcessor.OnStand += HandleStand;

            _runProcessor.OnStartRun += HandleStartRun;
            _runProcessor.OnStopRun += HandleStopRun;

            _motionInputProcessor.OnStartMoving += HandleStartMoving;
            _motionInputProcessor.OnStopMoving += HandleStopMoving;
        }

        private void OnDisable() {
            _massProcessor.OnFell -= HandleFell;
            _massProcessor.OnLanded -= HandleLanded;
            
            _jumpProcessor.OnJump -= HandleJump;
            
            _poseProcessor.OnCrouch -= HandleCrouch;
            _poseProcessor.OnStand -= HandleStand;
            
            _runProcessor.OnStartRun -= HandleStartRun;
            _runProcessor.OnStopRun -= HandleStopRun;
            
            _motionInputProcessor.OnStartMoving -= HandleStartMoving;
            _motionInputProcessor.OnStopMoving -= HandleStopMoving;
        }

        private void HandleStopMoving() {
            OnStopMoving.Invoke();
        }

        private void HandleStartMoving() {
            OnStartMoving.Invoke();
        }

        private void HandleStand() {
            OnStand.Invoke();
        }

        private void HandleCrouch() {
            OnCrouch.Invoke();
        }

        private void HandleStopRun() {
           OnStopRun.Invoke();
        }

        private void HandleStartRun() {
           OnStartRun.Invoke();
        }

        private void HandleJump(Vector3 impulse) {
            OnJump.Invoke(impulse);
        }

        private void HandleLanded(Vector3 force) {
            OnLanded.Invoke(force);
        }

        private void HandleFell() {
            OnFell.Invoke();
        }
    }

}