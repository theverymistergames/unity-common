using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Input.Bindings {

    public abstract class InputBindingVector2Base : InputBinding {

        [SerializeField] private float _sensitivityX = 1f;
        [SerializeField] private float _sensitivityY = 1f;
        [SerializeField] private bool _normalize = true;
        
        private Vector2 _vector = Vector2.zero;

        public Vector2 GetValue() {
            if (_normalize && !_vector.IsNearlyZero()) _vector.Normalize();

            _vector = GetVector();
            _vector.x *= _sensitivityX;
            _vector.y *= _sensitivityY;

            return _vector;
        }
        
        protected abstract Vector2 GetVector();
        
    }

}