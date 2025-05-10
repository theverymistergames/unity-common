using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Tick;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Logic.Phys {
    
    public sealed class EnableObjectsByGravityDirection : MonoBehaviour, IUpdate {

        [Header("Angle")]
        [SerializeField] private Transform _compareAngleTransform;
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] [MinMaxSlider(0f, 180f)] private Vector2 _angleRange;
        [SerializeField] private bool _addRangeForInvertedDirection;
        
        [Header("Objects")]
        [SerializeField] private bool _enableOnAngleMatch = true;
        [SerializeField] private Object[] _objects;
        
        private bool _isAngleMatch;
        private bool _forceUpdate;
        
        private void OnEnable() {
            _forceUpdate = true;
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var gravity = CustomGravity.Main.GetGlobalGravity(_compareAngleTransform.position, Physics.gravity);
            float angle = Vector3.Angle(GetForward(), gravity);
            
            bool match = angle >= _angleRange.x && angle <= _angleRange.y ||
                         _addRangeForInvertedDirection && angle <= 180f - _angleRange.x && angle >= 180f - _angleRange.y; 
            
            if (_isAngleMatch == match && !_forceUpdate) return;
            
            _isAngleMatch = match;
            _forceUpdate = false;
            
            _objects.SetEnabled(match == _enableOnAngleMatch);
        }

        private Vector3 GetForward() {
            return _compareAngleTransform.rotation * Quaternion.Euler(_rotationOffset) * Vector3.forward;
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void Reset() {
            _compareAngleTransform = transform;
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo || _compareAngleTransform == null) return;
            
            var p = _compareAngleTransform.position;
            var f = GetForward();
            
            DebugExt.DrawSphere(p, 0.05f, Color.yellow, gizmo: true);
            DebugExt.DrawRay(p, f, Color.yellow, gizmo: true);

            var color0 = _enableOnAngleMatch ? Color.green : Color.cyan;
            var color1 = _enableOnAngleMatch ? Color.yellow : Color.magenta;
            
            DebugExt.DrawVortex(p, f, _angleRange.x, color0, size: 0.25f, gizmo: true);
            DebugExt.DrawVortex(p, f, _angleRange.y, color0, size: 0.25f, gizmo: true);
            
            if (_addRangeForInvertedDirection) {
                DebugExt.DrawVortex(p, -f, _angleRange.x, color1, size: 0.25f, gizmo: true);
                DebugExt.DrawVortex(p, -f, _angleRange.y, color1, size: 0.25f, gizmo: true);
            }
        }
#endif
    }
    
}