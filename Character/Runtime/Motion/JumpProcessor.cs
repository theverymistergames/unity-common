﻿using System;
 using MisterGames.Character.Configs;
 using MisterGames.Character.Input;
 using MisterGames.Common.Collisions.Core;
 using MisterGames.Fsm.Core;
 using UnityEngine;

 namespace MisterGames.Character.Motion {

    public class JumpProcessor : MonoBehaviour {

        [Header("Controls")]
        [SerializeField] private CharacterInput _input;
        [SerializeField] private CharacterAdapter _adapter;
        
        [Header("States")]
        [SerializeField] private StateMachineRunner _motionFsm;
        [SerializeField] private StateMachineRunner _poseFsm;

        [Header("Collision")]
        [SerializeField] private CollisionDetector _groundDetector;

        public event Action<Vector3> OnJump = delegate {  };
        
        private readonly Vector3 jumpDirection = Vector3.up;
        
        private bool _isGrounded;
        private bool _isJumpAllowed;
        private bool _isJumpAllowedByMotion;
        private bool _isJumpAllowedByPose;
        private float _jumpForce;

        private void OnEnable() {
            _input.Jump += HandleJumpInput;
            
            _groundDetector.OnLostContact += HandleFell;
            _groundDetector.OnContact += HandleLanded;

            _motionFsm.OnEnterState += HandleMotionStateChanged;
            _poseFsm.OnEnterState += HandlePoseStateChanged;
        }

        private void OnDisable() {
            _input.Jump -= HandleJumpInput;
            
            _groundDetector.OnLostContact -= HandleFell;
            _groundDetector.OnContact -= HandleLanded;

            _motionFsm.OnEnterState -= HandleMotionStateChanged;
            _poseFsm.OnEnterState -= HandlePoseStateChanged;
        }

        private void Start() {
            HandleMotionStateChanged(_motionFsm.Instance.CurrentState);
            HandlePoseStateChanged(_poseFsm.Instance.CurrentState);
        }

        private void HandleJumpInput() {
            if (!_isJumpAllowed) return;
            var impulse = jumpDirection * _jumpForce;
            _adapter.ApplyImpulse(impulse);
            OnJump.Invoke(impulse);
        }

        private void HandleFell() {
            _isGrounded = false;
            InvalidateJumpAllowed();
        }

        private void HandleLanded() {
            _isGrounded = true;
            InvalidateJumpAllowed();
        }

        private void HandleMotionStateChanged(FsmState state) {
            if (state.data is MotionStateData data) {
                _isJumpAllowedByMotion = data.isJumpAllowedGrounded;
                _jumpForce = data.jumpForce;
                InvalidateJumpAllowed();
            }
        }

        private void HandlePoseStateChanged(FsmState state) {
            if (state.data is PoseStateData data) {
                _isJumpAllowedByPose = data.isJumpAllowed;
                InvalidateJumpAllowed();
            }
        }

        private void InvalidateJumpAllowed() {
            _isJumpAllowed = _isGrounded && _isJumpAllowedByMotion && _isJumpAllowedByPose;
        }

    }

}
