using System;
using MisterGames.Common.Maths;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    [Serializable]
    public sealed class FourKeyVector2 : IVector2Binding {

        [Header("Bindings")]
        [SerializeField] private KeyBinding _positiveX;
        [SerializeField] private KeyBinding _negativeX;

        [SerializeField] private KeyBinding _positiveY;
        [SerializeField] private KeyBinding _negativeY;

        [Header("Settings")]
        [SerializeField] private float _sensitivityX = 1f;
        [SerializeField] private float _sensitivityY = 1f;
        [SerializeField] private bool _normalize = true;

        public Vector2 Value {
            get {
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

}
