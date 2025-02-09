using System;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.View {

    [Serializable]
    public sealed class ViewClampProcessor {

        [SerializeField] private ViewAxisClamp _horizontal;
        [SerializeField] private ViewAxisClamp _vertical = new() {
            absolute = true,
            mode = ClampMode.Full, 
            bounds = new Vector2(-90f, 90f)
        };

        private LookMode _lookMode;
        private Transform _lookTarget;
        private Vector3 _lookTargetPoint;
        private Quaternion _lookTargetOrientation;
        private Quaternion _lookTargetOrientationSmoothed;
        private float _lookTargetSmoothing;

        private Vector2 _viewCenterStatic;
        private Vector2 _viewCenter;
        private float _viewCenterOffsetMul = 1f;

        private enum LookMode {
            Free,
            Point,
            Orientation,
            Transform,
            TransformOriented,
        }

        public void LookAt(Transform target, Vector2 startOrientation, LookAtMode mode, Vector3 offset, Vector3 orientation, float smoothing) {
            _lookMode = mode switch {
                LookAtMode.Free => LookMode.Transform,
                LookAtMode.Oriented => LookMode.TransformOriented,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
            
            _lookTarget = target;
            _lookTargetPoint = offset;
            _lookTargetOrientationSmoothed = Quaternion.Euler(startOrientation);
            _lookTargetOrientation = Quaternion.Euler(orientation);
            _lookTargetSmoothing = smoothing;
        }

        public void LookAt(Vector3 target, Vector2 startOrientation, float smoothing) {
            _lookMode = LookMode.Point;
            
            _lookTarget = null;
            _lookTargetPoint = target;
            _lookTargetOrientationSmoothed = Quaternion.Euler(startOrientation);
            _lookTargetSmoothing = smoothing;
        }
        
        public void LookAlong(Quaternion orientation, Vector2 startOrientation, float smoothing) {
            _lookMode = LookMode.Orientation;
            
            _lookTarget = null;
            _lookTargetOrientation = orientation;
            _lookTargetOrientationSmoothed = Quaternion.Euler(startOrientation);
            _lookTargetSmoothing = smoothing;
        }

        public void StopLookAt() {
            _lookMode = LookMode.Free;
            _lookTarget = null;
        }

        public void ApplyHorizontalClamp(ViewAxisClamp clamp, Vector2 orientation) {
            _horizontal = clamp;
            _viewCenterStatic = orientation;
            ResetNextViewCenterOffset();
        }

        public void ApplyVerticalClamp(ViewAxisClamp clamp, Vector2 orientation) {
            _vertical = clamp;
            _viewCenterStatic = orientation;
            ResetNextViewCenterOffset();
        }

        public void SetViewOrientation(Vector2 orientation) {
            _viewCenterStatic = orientation;
        }

        public void ResetNextViewCenterOffset() {
            _viewCenterOffsetMul = 0;
        }

        public void Process(
            Vector3 position,
            Quaternion rotationOffset,
            ref Vector2 orientation,
            ref Vector2 targetOrientation,
            float dt
        ) {
            targetOrientation = targetOrientation.ToEulerAngles180();
            
            var lastViewCenter = _viewCenter;
            _viewCenter = GetViewCenter(position, rotationOffset, dt).GetNearestAngle(targetOrientation);

            var diff = _viewCenterOffsetMul * (_viewCenter - lastViewCenter);
            orientation += diff;
            targetOrientation = (targetOrientation + diff).GetNearestAngle(_viewCenter);

            var verticalBounds = _vertical.bounds + Vector2.one * _viewCenter.x;
            var horizontalBounds = _horizontal.bounds + Vector2.one * _viewCenter.y;
            
            _viewCenterOffsetMul = 1f;
            
            targetOrientation.x = targetOrientation.x.Clamp(_vertical.mode, verticalBounds.x, verticalBounds.y);
            targetOrientation.y = targetOrientation.y.Clamp(_horizontal.mode, horizontalBounds.x, horizontalBounds.y);
            
            targetOrientation.x = ApplySpring(orientation.x, targetOrientation.x, _viewCenter.x, _vertical, dt);
            targetOrientation.y = ApplySpring(orientation.y, targetOrientation.y, _viewCenter.y, _horizontal, dt);
        }

        private Vector2 GetViewCenter(Vector3 position, Quaternion rotationOffset, float dt) {
            Vector2 viewCenter;
            
            switch (_lookMode) {
                case LookMode.Free: {
                    viewCenter = _viewCenterStatic;
                    break;
                }

                case LookMode.Point: {
                    var targetRot = Quaternion.Inverse(rotationOffset) * 
                                    Quaternion.LookRotation(_lookTargetPoint - position, Vector3.up);
                    
                    _lookTargetOrientationSmoothed = _lookTargetOrientationSmoothed
                        .SlerpNonZero(targetRot, _lookTargetSmoothing, dt);
                    
                    viewCenter = _lookTargetOrientationSmoothed.eulerAngles;
                    break;
                }
                
                case LookMode.Orientation: {
                    _lookTargetOrientationSmoothed = _lookTargetOrientationSmoothed
                        .SlerpNonZero(_lookTargetOrientation, _lookTargetSmoothing, dt);
                    
                    viewCenter = _lookTargetOrientationSmoothed.eulerAngles;
                    break;
                }

                case LookMode.Transform: {
                    var targetRot = Quaternion.Inverse(rotationOffset) * 
                                    Quaternion.LookRotation(_lookTarget.TransformPoint(_lookTargetPoint) - position, Vector3.up);
                    
                    _lookTargetOrientationSmoothed = _lookTargetOrientationSmoothed
                        .SlerpNonZero(targetRot, _lookTargetSmoothing, dt);

                    viewCenter = _lookTargetOrientationSmoothed.eulerAngles;
                    break;
                }

                case LookMode.TransformOriented: {
                    var targetRot = Quaternion.Inverse(rotationOffset) * 
                                    _lookTarget.rotation * _lookTargetOrientation;
                    
                    _lookTargetOrientationSmoothed = _lookTargetOrientationSmoothed
                        .SlerpNonZero(targetRot, _lookTargetSmoothing, dt);

                    viewCenter = _lookTargetOrientationSmoothed.eulerAngles;
                    break;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            viewCenter.x *= (!_vertical.absolute).AsInt();
            viewCenter.y *= (!_horizontal.absolute).AsInt();

            return viewCenter;
        }
        
        private static float ApplySpring(float value, float target, float center, ViewAxisClamp clamp, float dt) {
            float diff = target - value;
            float centralizedTarget = target - center;
            
            float lw = 0f;
            float rw = 0f;

            float lt = target;
            float rt = target;
            
            // In left spring zone
            if (centralizedTarget < clamp.springs.x && 
                clamp.mode is ClampMode.Lower or ClampMode.Full && 
                clamp.springFactors.x > 0f
            ) {
                lw = Mathf.Clamp01(1f - (value - center - clamp.bounds.x) / (clamp.springs.x - clamp.bounds.x));
                lt = diff >= 0f
                    ? centralizedTarget.SmoothExp(clamp.springs.x, dt * clamp.springFactors.x) + center
                    : value + diff.SmoothExp(0f, clamp.springFactors.x * lw);
            }

            // In right spring zone
            if (centralizedTarget > clamp.springs.y && 
                clamp.mode is ClampMode.Upper or ClampMode.Full && 
                clamp.springFactors.y > 0f
            ) {
                rw = Mathf.Clamp01(1f - (value - center - clamp.bounds.y) / (clamp.springs.y - clamp.bounds.y));
                rt = diff <= 0f
                    ? centralizedTarget.SmoothExp(clamp.springs.y, dt * clamp.springFactors.y) + center
                    : value + diff.SmoothExp(0f, clamp.springFactors.y * rw);
            }

            return lw + rw <= 0f ? target : (lw * lt + rw * rt) / (lw + rw);
        }
    }

}
