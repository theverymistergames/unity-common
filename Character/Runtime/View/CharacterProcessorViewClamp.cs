using System;
using MisterGames.Character.Processors;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.View {

    [Serializable]
    public sealed class CharacterProcessorViewClamp : ICharacterProcessorVector2 {

        public ViewAxisClamp horizontal;
        public ViewAxisClamp vertical;

        private Vector2 _lastResult;
        private Vector2 _offset;

        public void ApplyVerticalClamp(ViewAxisClamp clamp) {
            vertical = clamp;
            _offset = _lastResult;
        }

        public void ApplyHorizontalClamp(ViewAxisClamp clamp) {
            horizontal = clamp;
            _offset = _lastResult;
        }
        
        public Vector2 Process(Vector2 input, float dt) {
            var result = new Vector2(
                (input.x - _offset.x).Clamp(vertical.mode, vertical.bounds.x, vertical.bounds.y),
                (input.y - _offset.y).Clamp(horizontal.mode, horizontal.bounds.x, horizontal.bounds.y)
            ) + _offset;

            var diff = result - _lastResult;

            result.x = ApplySpring(_lastResult.x, diff.x, vertical, dt);
            result.y = ApplySpring(_lastResult.y, diff.y, horizontal, dt);

            _lastResult = result;

            return result;
        }

        private float ApplySpring(float value, float diff, ViewAxisClamp clamp, float dt) {
            if (clamp.mode == ClampMode.None) return value + diff;

            float nextValue = value + diff;

            // Not in spring zone
            if (nextValue >= clamp.springs.x && nextValue <= clamp.springs.y) return nextValue;

            // In left spring zone
            if (nextValue < clamp.springs.x && clamp.mode is ClampMode.Lower or ClampMode.Full) {
                // Ignore if spring factor is not set
                if (clamp.springFactors.x <= 0f) return nextValue;

                // Not moving or moving towards spring: lerp to spring
                if (diff >= 0f) return Mathf.Lerp(nextValue, clamp.springs.x, dt * clamp.springFactors.x);

                // Moving towards bound: decrease diff depending on distance between value and bound
                return value + diff * (value - clamp.bounds.x) / (clamp.springs.x - clamp.bounds.x);
            }

            // In right spring zone
            if (nextValue > clamp.springs.y && clamp.mode is ClampMode.Upper or ClampMode.Full) {
                // Ignore if spring factor is not set
                if (clamp.springFactors.y <= 0f) return nextValue;

                // Not moving or moving towards spring: lerp to spring
                if (diff <= 0f) return Mathf.Lerp(nextValue, clamp.springs.y, dt * clamp.springFactors.y);

                // Moving towards bound: decrease diff depending on distance between value and bound
                return value + diff * (value - clamp.bounds.y) / (clamp.springs.y - clamp.bounds.y);
            }

            return nextValue;
        }
    }

}
