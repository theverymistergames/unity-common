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

        public ViewAxisClamp Horizontal => _horizontal;
        public ViewAxisClamp Vertical => _vertical;
        
        private Transform _lookTarget;
        private Vector3 _lookTargetPoint;
        private Vector2 _clampCenterEulers;
        private LookMode _lookMode;

        private enum LookMode {
            Free,
            Point,
            Transform
        }

        public void LookAt(Transform target) {
            _lookTarget = target;
            _lookMode = LookMode.Transform;
        }

        public void LookAt(Vector3 target) {
            _lookTarget = null;
            _lookTargetPoint = target;
            _lookMode = LookMode.Point;
        }

        public void StopLookAt() {
            _lookTarget = null;
            _lookMode = LookMode.Free;
        }

        public void ApplyHorizontalClamp(Vector2 orientation, ViewAxisClamp clamp) {
            _clampCenterEulers.y = orientation.y;
            _horizontal = clamp;
        }

        public void ApplyVerticalClamp(Vector2 orientation, ViewAxisClamp clamp) {
            _clampCenterEulers.x = orientation.x;
            _vertical = clamp;
        }

        public void Process(Vector3 position, Vector2 orientation, ref Vector2 targetOrientation, float dt) {
            Vector2 clampCenterEulers = _lookMode switch {
                LookMode.Free => _clampCenterEulers,
                LookMode.Point => Quaternion.LookRotation(_lookTargetPoint - position, Vector3.up).eulerAngles,
                LookMode.Transform => Quaternion.LookRotation(_lookTarget.position - position, Vector3.up).eulerAngles, 
            };

            clampCenterEulers = GetNearestAngle(clampCenterEulers, targetOrientation);

            clampCenterEulers.x *= (!_vertical.absolute).AsInt();
            clampCenterEulers.y *= (!_horizontal.absolute).AsInt();
            
            var verticalBounds = _vertical.bounds + Vector2.one * clampCenterEulers.x;
            var horizontalBounds = _horizontal.bounds + Vector2.one * clampCenterEulers.y;

            targetOrientation.x = targetOrientation.x.Clamp(_vertical.mode, verticalBounds.x, verticalBounds.y);
            targetOrientation.y = targetOrientation.y.Clamp(_horizontal.mode, horizontalBounds.x, horizontalBounds.y);

            targetOrientation.x = ApplySpring(orientation.x, targetOrientation.x, clampCenterEulers.x, _vertical, dt);
            targetOrientation.y = ApplySpring(orientation.y, targetOrientation.y, clampCenterEulers.y, _horizontal, dt);

            targetOrientation = GetNearestAngle(targetOrientation, orientation);
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
            
            // Not clamped or not in spring zone
            if (clamp.mode == ClampMode.None ||
                centralizedTarget >= clamp.springs.x && centralizedTarget <= clamp.springs.y
            ) {
                return target;
            }

            // In left spring zone
            if (centralizedTarget < clamp.springs.x && clamp.mode is ClampMode.Lower or ClampMode.Full) {
                // Ignore if spring factor is not set
                if (clamp.springFactors.x <= 0f) return target;

                // Not moving or moving towards spring: lerp to spring
                if (diff >= 0f) return Mathf.Lerp(centralizedTarget, clamp.springs.x, dt * clamp.springSmoothing.x) + center;

                // Moving towards bound: decrease diff depending on distance between value and bound
                float f = 1f - (value - center - clamp.bounds.x) / (clamp.springs.x - clamp.bounds.x);
                return value + Mathf.Lerp(diff, 0f, clamp.springFactors.x * f);
            }

            // In right spring zone
            if (centralizedTarget > clamp.springs.y && clamp.mode is ClampMode.Upper or ClampMode.Full) {
                // Ignore if spring factor is not set
                if (clamp.springFactors.y <= 0f) return target;

                // Not moving or moving towards spring: lerp to spring
                if (diff <= 0f) return Mathf.Lerp(centralizedTarget, clamp.springs.y, dt * clamp.springSmoothing.y) + center;

                // Moving towards bound: decrease diff depending on distance between value and bound
                float f = 1f - (value - center - clamp.bounds.y) / (clamp.springs.y - clamp.bounds.y);
                return value + Mathf.Lerp(diff, 0f, clamp.springFactors.y * f);
            }

            return target;
        }
    }

}
