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
                    _positiveX.IsActive().AsInt() - _negativeX.IsActive().AsInt(),
                    _positiveY.IsActive().AsInt() - _negativeY.IsActive().AsInt()
                );

                if (_normalize && vector != Vector2.zero) vector.Normalize();

                vector.x *= _sensitivityX;
                vector.y *= _sensitivityY;

                return vector;
            }
        }
    }

}
