﻿using MisterGames.Character.Configs;
 using UnityEngine;

 namespace MisterGames.Character.Motion {

    public class SpeedProcessor {
        
        public float Speed { get; private set; }

        private Vector2 _inputDirection;
        private float _speedInput;
        private float _speedCorr = 1f;
        private float _speedCorrSide = 1f;
        private float _speedCorrBack = 1f;

        public void SetInputDirection(Vector2 direction) {
            _inputDirection = direction;
            _speedCorr = CalculateSpeedCorrection();
            InvalidateSpeed();
        }
        
        public void SetMotionData(MotionStateData data) {
            _speedInput = data.speed; 
            _speedCorrSide = data.sideCorrection;
            _speedCorrBack = data.backCorrection;
            _speedCorr = CalculateSpeedCorrection();
            InvalidateSpeed();
        }
        
        private void InvalidateSpeed() {
            Speed = _speedInput * _speedCorr;
        }
        
        private float CalculateSpeedCorrection() {
            // Moving backwards OR backwards + sideways
            if (_inputDirection.y < 0) return _speedCorrBack;
            
            // Moving forwards OR forwards + sideways: no adjustment
            if (_inputDirection.y > 0) return 1f;
            
            // Moving sideways only
            return _speedCorrSide;
        }
        
    }

}