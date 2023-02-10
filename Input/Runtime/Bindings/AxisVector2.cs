using System;
using MisterGames.Common.Maths;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    [Serializable]
    public sealed class AxisVector2 : IVector2Binding {

        [Header("Bindings")]
        [SerializeField] private AxisBinding _axis;

        [Header("Settings")]
        [SerializeField] private float _sensitivityX = 1f;
        [SerializeField] private float _sensitivityY = 1f;
        [SerializeField] private bool _normalize = true;

        public Vector2 Value {
            get {
                var vector = _axis.GetValue();

                if (_normalize && !vector.IsNearlyZero()) vector.Normalize();

                vector.x *= _sensitivityX;
                vector.y *= _sensitivityY;

                return vector;
            }
        }
    }
}
