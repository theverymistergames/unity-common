using System;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.View {

    [Serializable]
    public sealed class CharacterViewClampProcessor {

        [SerializeField] private ViewAxisClamp _horizontal;
        [SerializeField] private ViewAxisClamp _vertical = new() {
            mode = ClampMode.Full, 
            bounds = new Vector2(-90f, 90f)
        };

        private Transform _lookTarget;
        private Vector3 _lookTargetPoint;
        private Vector3 _clampCenterEulers;
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
            _clampCenterEulers.y = clamp.absolute ? 0f : orientation.y;
            _horizontal = clamp;
        }

        public void ApplyVerticalClamp(Vector2 orientation, ViewAxisClamp clamp) {
            _clampCenterEulers.x = clamp.absolute ? 0f : orientation.x;
            _vertical = clamp;
        }

        public void Process(Vector3 position, Vector2 orientation, ref Vector2 targetOrientation, float dt) {
            var clampCenterEulers = _lookMode switch {
                LookMode.Free => _clampCenterEulers,
                LookMode.Point => Quaternion.FromToRotation(Vector3.forward, _lookTargetPoint - position).eulerAngles,
                LookMode.Transform => Quaternion.FromToRotation(Vector3.forward, _lookTarget.position - position).eulerAngles,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var verticalBounds = _vertical.bounds + Vector2.one * clampCenterEulers.x;
            var horizontalBounds = _horizontal.bounds + Vector2.one * clampCenterEulers.y;
            
            targetOrientation.x = targetOrientation.x.Clamp(_vertical.mode, verticalBounds.x, verticalBounds.y);
            targetOrientation.y = targetOrientation.y.Clamp(_horizontal.mode, horizontalBounds.x, horizontalBounds.y);

            targetOrientation.x = ApplySpring(orientation.x, targetOrientation.x - orientation.x, clampCenterEulers.x, _vertical, dt);
            targetOrientation.y = ApplySpring(orientation.y, targetOrientation.y - orientation.y, clampCenterEulers.y, _horizontal, dt);
        }
        
        private static float ApplySpring(float value, float diff, float center, ViewAxisClamp clamp, float dt) {
            float target = value + diff - center;
            
            // Not clamped or not in spring zone
            if (clamp.mode == ClampMode.None ||
                target >= clamp.springs.x && target <= clamp.springs.y
            ) {
                return target + center;
            }

            // In left spring zone
            if (target < clamp.springs.x && clamp.mode is ClampMode.Lower or ClampMode.Full) {
                // Ignore if spring factor is not set
                if (clamp.springFactors.x <= 0f) return target + center;

                // Not moving or moving towards spring: lerp to spring
                if (diff >= 0f) return Mathf.Lerp(target, clamp.springs.x, dt * clamp.springFactors.x) + center;

                // Moving towards bound: decrease diff depending on distance between value and bound
                float f = (value - center - clamp.bounds.x) / (clamp.springs.x - clamp.bounds.x);
                return value + diff * f * f;
            }

            // In right spring zone
            if (target > clamp.springs.y && clamp.mode is ClampMode.Upper or ClampMode.Full) {
                // Ignore if spring factor is not set
                if (clamp.springFactors.y <= 0f) return target + center;

                // Not moving or moving towards spring: lerp to spring
                if (diff <= 0f) return Mathf.Lerp(target, clamp.springs.y, dt * clamp.springFactors.y) + center;

                // Moving towards bound: decrease diff depending on distance between value and bound
                float f = (value - center - clamp.bounds.y) / (clamp.springs.y - clamp.bounds.y);
                return value + diff * f * f;
            }

            return target + center;
        }
    }

}
