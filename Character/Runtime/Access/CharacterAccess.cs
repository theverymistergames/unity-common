﻿using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.UI.Initialization;
using UnityEngine;

namespace MisterGames.Character.Access {
    
    public class CharacterAccess : MonoBehaviour {

        [SerializeField] private CharacterAdapter _characterAdapter;
        [SerializeField] private CameraController _cameraController;

        public static CharacterAccess Instance { get; private set; }

        private void Awake() {
            Instance = this;
            CanvasRegistry.Instance.SetCanvasEventCamera(_cameraController.Camera);
        }

        private void OnDestroy() {
            Instance = null;
            CanvasRegistry.Instance.SetCanvasEventCamera(null);
        }

        public void SetPosition(Vector3 position) {
            _characterAdapter.SetPosition(position);
        }
        
        public void SetRotation(Quaternion rotation) {
            
        }

        public void ResetMotionState() {
            
        }
        
        public void ResetPoseState() {
            
        }

        
        public void SetGravityForceOverride(float gravityForce) {
            
        }

        public void ResetGravityForceOverride() {
            
        }

        public void SetGravityForceMultiplier(float gravityForceMultiplier) {
            
        }
        
        public void ResetGravityForceMultiplier() {
            
        }
        
        public void ResetGravity() {
            
        }
       
        
        public void SetAirInertialFactorOverride(float airInertialFactor) {
            
        }

        public void ResetAirInertialFactorOverride() {
            
        }

        public void SetAirInertialFactorMultiplier(float airInertialFactorMultiplier) {
            
        }
        
        public void ResetAirInertialFactorMultiplier() {
            
        }
        
        
        public void SetGroundInertialFactorOverride(float groundInertialFactor) {
            
        }

        public void ResetGroundInertialFactorOverride() {
            
        }

        public void SetGroundInertialFactorMultiplier(float groundInertialFactorMultiplier) {
            
        }
        
        public void ResetGroundInertialFactorMultiplier() {
            
        }
        
        
        public void SetMotionSmoothFactorOverride(float motionSmoothFactor) {
            
        }

        public void ResetMotionSmoothFactorOverride() {
            
        }
        
        public void SetMotionSmoothFactorMultiplier(float motionSmoothFactorMultiplier) {
            
        }

        public void ResetMotionSmoothFactorMultiplier() {
            
        }
        
        
        public void SetViewSmoothFactorOverride(float viewSmoothFactor) {
            
        }

        public void ResetViewSmoothFactorOverride() {
            
        }
        
        public void SetViewSmoothFactorMultiplier(float viewSmoothFactorMultiplier) {
            
        }

        public void ResetViewSmoothFactorMultiplier() {
            
        }
        
        
        public void SetViewSensitivityHorizontalOverride(float sensitivityHorizontal) {
            
        }

        public void ResetViewSensitivityHorizontalOverride() {
            
        }
        
        public void SetViewSensitivityHorizontalMultiplier(float sensitivityHorizontalMultiplier) {
            
        }

        public void ResetViewSensitivityHorizontalMultiplier() {
            
        }
        
        
        public void SetViewSensitivityVerticalOverride(float sensitivityVertical) {
            
        }

        public void ResetViewSensitivityVerticalOverride() {
            
        }
        
        public void SetViewSensitivityVerticalMultiplier(float sensitivityVerticalMultiplier) {
            
        }

        public void ResetViewSensitivityVerticalMultiplier() {
            
        }

    }
    
}
