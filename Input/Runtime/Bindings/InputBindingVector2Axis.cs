using System;
using MisterGames.Common.Maths;
using MisterGames.Input.Core;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    public interface IInputBindingVector2 : IInputBinding {
        Vector2 GetValue();
    }

    [Serializable]
    public class InputBindingVector2Axis : IInputBindingVector2 {

        [Header("Bindings")]
        [SerializeField] private AxisBinding _axis;

        [Header("Settings")]
        [SerializeField] private float _sensitivityX = 1f;
        [SerializeField] private float _sensitivityY = 1f;
        [SerializeField] private bool _normalize = true;

        public void Init() { }

        public void Terminate() { }

        public Vector2 GetValue() {
            var vector = _axis.GetValue();

            if (_normalize && !vector.IsNearlyZero()) vector.Normalize();

            vector.x *= _sensitivityX;
            vector.y *= _sensitivityY;

            return vector;
        }
    }

    [Serializable]
    public class InputBindingVector2Key : IInputBindingVector2 {

        [Header("Bindings")]
        [SerializeField] private KeyBinding _positiveX;
        [SerializeField] private KeyBinding _negativeX;

        [SerializeField] private KeyBinding _positiveY;
        [SerializeField] private KeyBinding _negativeY;

        [Header("Settings")]
        [SerializeField] private float _sensitivityX = 1f;
        [SerializeField] private float _sensitivityY = 1f;
        [SerializeField] private bool _normalize = true;

        public void Init() { }

        public void Terminate() { }

        public Vector2 GetValue() {
            var vector = new Vector2(
                _positiveX.IsActive().ToInt() - _negativeX.IsActive().ToInt(),
                _positiveY.IsActive().ToInt() - _negativeY.IsActive().ToInt()
            );

            if (_normalize && !vector.IsNearlyZero()) vector.Normalize();

            vector.x *= _sensitivityX;
            vector.y *= _sensitivityY;

            return vector;
        }
    }
}
