using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    public sealed class ColliderWeightProvider : PositionWeightProvider {
    
        [SerializeField] private Collider _collider;
        [SerializeField] [Min(0f)] private float _blendDistance = 1f;
        [SerializeField] private float _weightMul = 1f;
        [SerializeField] private AnimationCurve _weightCurve = EasingType.Linear.ToAnimationCurve();
        
        public override float GetWeight(Vector3 position) {
            return _weightMul * _weightCurve.Evaluate(GetLinearWeight(position));
        }

        private float GetLinearWeight(Vector3 position) {
            var p = _collider.ClosestPoint(position);

            if (p == position) {
                return 1f;
            }
            
            if (Vector3.SqrMagnitude(position - p) > _blendDistance * _blendDistance) {
                return 0f;
            }
            
            float distance = Vector3.Distance(p, position);
            return _blendDistance > 0f ? Mathf.Clamp01(1f - distance / _blendDistance) : 1f;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        [VisibleIf(nameof(_showDebugInfo))]
        [SerializeField] private Vector3 _testPoint;

        private void OnDrawGizmos() {
            if (!_showDebugInfo) return;

            var p = transform.TransformPoint(_testPoint);
            var b = _collider.ClosestPointOnBounds(p);
            
            DebugExt.DrawSphere(p, 0.05f, Color.white);
            DebugExt.DrawLine(p, b, Color.white);
            
            DebugExt.DrawLabel(p + transform.up * 0.1f, $"W = {GetWeight(p):0.000}\nLin = {GetLinearWeight(p):0.000}");
        }
#endif
    }
    
}