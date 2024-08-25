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
        private Vector2 _clampCenterEulers;
        private LookMode _lookMode;

        private enum LookMode {
            Free,
            Point,
            Transform,
            TransformOriented,
        }

        public void LookAt(Transform target, Vector2 startOrientation, LookAtMode mode, Vector3 orientation, float smoothing) {
            _lookTarget = target;
            _lookTargetPoint = target.position;
            _lookTargetOrientationSmoothed = Quaternion.Euler(startOrientation);
            _lookTargetOrientation = Quaternion.Euler(orientation);
            _lookTargetSmoothing = smoothing;
            
            _lookMode = mode switch {
                LookAtMode.Free => LookMode.Transform,
                LookAtMode.Oriented => LookMode.TransformOriented,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        public void LookAt(Vector3 target, Vector2 startOrientation, float smoothing) {
            _lookTarget = null;
            _lookTargetPoint = target;
            _lookTargetOrientationSmoothed = Quaternion.Euler(startOrientation);
            _lookTargetSmoothing = smoothing;
            
            _lookMode = LookMode.Point;
        }

        public void StopLookAt() {
            _lookTarget = null;
            _lookMode = LookMode.Free;
        }

        public void ApplyHorizontalClamp(ViewAxisClamp clamp) {
            _horizontal = clamp;
        }

        public void ApplyVerticalClamp(ViewAxisClamp clamp) {
            _vertical = clamp;
        }

        public void SetClampCenter(Vector2 orientation) {
            _clampCenterEulers = orientation;
        }

        public void Process(Vector3 position, Vector2 orientation, ref Vector2 targetOrientation, float dt) {
            var clampCenterEulers = GetViewCenter(position, targetOrientation, dt);
            
            var verticalBounds = _vertical.bounds + Vector2.one * clampCenterEulers.x;
            var horizontalBounds = _horizontal.bounds + Vector2.one * clampCenterEulers.y;

            targetOrientation.x = targetOrientation.x.Clamp(_vertical.mode, verticalBounds.x, verticalBounds.y);
            targetOrientation.y = targetOrientation.y.Clamp(_horizontal.mode, horizontalBounds.x, horizontalBounds.y);

            targetOrientation.x = ApplySpring(orientation.x, targetOrientation.x, clampCenterEulers.x, _vertical, dt);
            targetOrientation.y = ApplySpring(orientation.y, targetOrientation.y, clampCenterEulers.y, _horizontal, dt);

            targetOrientation = GetNearestAngle(targetOrientation, orientation);
        }

        private Vector2 GetViewCenter(Vector3 position, Vector2 targetOrientation, float dt) {
            Vector2 clampCenterEulers;
            
            switch (_lookMode) {
                case LookMode.Free: {
                    clampCenterEulers = _clampCenterEulers;
                    break;
                }

                case LookMode.Point: {
                    var targetRot = Quaternion.LookRotation(_lookTargetPoint - position, Vector3.up);
                    _lookTargetOrientationSmoothed = _lookTargetSmoothing > 0f
                        ? Quaternion.Slerp(_lookTargetOrientationSmoothed, targetRot, dt * _lookTargetSmoothing)
                        : targetRot;

                    clampCenterEulers = _lookTargetOrientationSmoothed.eulerAngles;
                    break;
                }

                case LookMode.Transform: {
                    var targetRot = Quaternion.LookRotation(_lookTarget.position - position, Vector3.up);
                    _lookTargetOrientationSmoothed = _lookTargetSmoothing > 0f
                        ? Quaternion.Slerp(_lookTargetOrientationSmoothed, targetRot, dt * _lookTargetSmoothing)
                        : targetRot;

                    clampCenterEulers = _lookTargetOrientationSmoothed.eulerAngles;
                    break;
                }

                case LookMode.TransformOriented: {
                    var targetRot = _lookTarget.rotation * _lookTargetOrientation;
                    _lookTargetOrientationSmoothed = _lookTargetSmoothing > 0f
                        ? Quaternion.Slerp(_lookTargetOrientationSmoothed, targetRot, dt * _lookTargetSmoothing)
                        : targetRot;

                    clampCenterEulers = _lookTargetOrientationSmoothed.eulerAngles;
                    break;
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            clampCenterEulers = GetNearestAngle(clampCenterEulers, targetOrientation);
            
            clampCenterEulers.x *= (!_vertical.absolute).AsInt();
            clampCenterEulers.y *= (!_horizontal.absolute).AsInt();

            return clampCenterEulers;
        }

        private Vector2 GetNearestAngle(Vector2 value, Vector2 target) {
            value = value.Mod(360f);
            var t = (target / 360f).FloorToInt() + new Vector2(value.x > 0f ? -1f : 0f, value.y > 0f ? -1f : 0f);
            
            var p0 = t * 360f + value;
            var p1 = (t + Vector2.one) * 360f + value;
            var p2 = (t + Vector2.one * 2f) * 360f + value;

            var p01 = new Vector2(
                Mathf.Abs(p0.x - target.x) < Mathf.Abs(p1.x - target.x) ? p0.x : p1.x,
                Mathf.Abs(p0.y - target.y) < Mathf.Abs(p1.y - target.y) ? p0.y : p1.y
            );
            
            value.x = Mathf.Abs(p01.x - target.x) < Mathf.Abs(p2.x - target.x) ? p01.x : p2.x;
            value.y = Mathf.Abs(p01.y - target.y) < Mathf.Abs(p2.y - target.y) ? p01.y : p2.y;

            return value;
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
