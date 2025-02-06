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

        private Transform _lookTarget;
        private Vector3 _lookTargetPoint;
        private Quaternion _lookTargetOrientation;
        private Quaternion _lookTargetOrientationSmoothed;
        private float _lookTargetSmoothing;
        private Vector2 _clampCenterStatic;
        private Vector2 _clampCenter;
        private LookMode _lookMode;
        private bool _ignoreNextClampCenterOffset;

        private enum LookMode {
            Free,
            Point,
            Transform,
            TransformOriented,
        }

        public void LookAt(Transform target, Vector2 startOrientation, LookAtMode mode, Vector3 orientation, float smoothing) {
            _lookMode = mode switch {
                LookAtMode.Free => LookMode.Transform,
                LookAtMode.Oriented => LookMode.TransformOriented,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
            
            _lookTarget = target;
            _lookTargetPoint = target.position;
            _lookTargetOrientationSmoothed = Quaternion.Euler(startOrientation);
            _lookTargetOrientation = Quaternion.Euler(orientation);
            _lookTargetSmoothing = smoothing;

            _ignoreNextClampCenterOffset = true;

            Debug.Log($"ViewClampProcessor.LookAt: f {Time.frameCount}, ");
        }

        public void LookAt(Vector3 target, Vector2 orientation, float smoothing) {
            _lookMode = LookMode.Point;
            
            _lookTarget = null;
            _lookTargetPoint = target;
            _lookTargetOrientationSmoothed = Quaternion.Euler(orientation);
            _lookTargetSmoothing = smoothing;
            
            _ignoreNextClampCenterOffset = true;

            Debug.Log($"ViewClampProcessor.LookAt: point f {Time.frameCount}, ");
        }

        public void StopLookAt(Vector2 orientation) {
            _lookMode = LookMode.Free;
            _lookTarget = null;
        }

        public void ApplyHorizontalClamp(ViewAxisClamp clamp, Vector2 orientation) {
            _horizontal = clamp;
        }

        public void ApplyVerticalClamp(ViewAxisClamp clamp, Vector2 orientation) {
            _vertical = clamp;
        }

        public void SetClampCenter(Vector2 orientation) {
            _clampCenterStatic = orientation;
        }

        public void Process(
            Vector3 position,
            Quaternion rotationOffset,
            ref Vector2 orientation,
            ref Vector2 targetOrientation,
            float dt
        ) {
            targetOrientation = targetOrientation.ToEulerAngles180();
            
            ProcessClampCenter(position, rotationOffset, ref orientation, ref targetOrientation, dt);
            
            var verticalBounds = _vertical.bounds + Vector2.one * _clampCenter.x;
            var horizontalBounds = _horizontal.bounds + Vector2.one * _clampCenter.y;
            
            targetOrientation.x = targetOrientation.x.Clamp(_vertical.mode, verticalBounds.x, verticalBounds.y);
            targetOrientation.y = targetOrientation.y.Clamp(_horizontal.mode, horizontalBounds.x, horizontalBounds.y);
            
            targetOrientation.x = ApplySpring(orientation.x, targetOrientation.x, _clampCenter.x, _vertical, dt);
            targetOrientation.y = ApplySpring(orientation.y, targetOrientation.y, _clampCenter.y, _horizontal, dt);
        }

        private void ProcessClampCenter(Vector3 position, Quaternion rotationOffset, ref Vector2 orientation, ref Vector2 targetOrientation, float dt) {
            var clampCenterLast = _clampCenter;
            _clampCenter = GetViewCenter(position, rotationOffset, dt).GetNearestAngle(targetOrientation);

            if (_lookMode != LookMode.Free && !_ignoreNextClampCenterOffset) {
                //orientation += _clampCenter - clampCenterLast;
                //targetOrientation = (targetOrientation + _clampCenter - clampCenterLast).GetNearestAngle(_clampCenter);
            }

            _ignoreNextClampCenterOffset = false;
        }

        private Vector2 GetViewCenter(Vector3 position, Quaternion rotationOffset, float dt) {
            Vector2 clampCenterEulers;
            
            switch (_lookMode) {
                case LookMode.Free: {
                    clampCenterEulers = _clampCenterStatic;
                    break;
                }

                case LookMode.Point: {
                    var targetRot = Quaternion.Inverse(rotationOffset) * 
                                    Quaternion.LookRotation(_lookTargetPoint - position, Vector3.up);
                    
                    _lookTargetOrientationSmoothed = _lookTargetSmoothing > 0f
                        ? Quaternion.Slerp(_lookTargetOrientationSmoothed, targetRot, dt * _lookTargetSmoothing)
                        : targetRot;

                    clampCenterEulers = _lookTargetOrientationSmoothed.eulerAngles;
                    break;
                }

                case LookMode.Transform: {
                    var targetRot = Quaternion.Inverse(rotationOffset) * 
                                    Quaternion.LookRotation(_lookTarget.position - position, Vector3.up);
                    
                    _lookTargetOrientationSmoothed = _lookTargetSmoothing > 0f
                        ? Quaternion.Slerp(_lookTargetOrientationSmoothed, targetRot, dt * _lookTargetSmoothing)
                        : targetRot;

                    clampCenterEulers = _lookTargetOrientationSmoothed.eulerAngles;
                    break;
                }

                case LookMode.TransformOriented: {
                    var targetRot = Quaternion.Inverse(rotationOffset) * 
                                    _lookTarget.rotation * _lookTargetOrientation;
                    
                    _lookTargetOrientationSmoothed = _lookTargetSmoothing > 0f
                        ? Quaternion.Slerp(_lookTargetOrientationSmoothed, targetRot, dt * _lookTargetSmoothing)
                        : targetRot;

                    clampCenterEulers = _lookTargetOrientationSmoothed.eulerAngles;
                    break;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            clampCenterEulers.x *= (!_vertical.absolute).AsInt();
            clampCenterEulers.y *= (!_horizontal.absolute).AsInt();

            return clampCenterEulers;
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
