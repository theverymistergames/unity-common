using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.Logic.Transforms {
    
    public sealed class RadialWeightProvider : PositionWeightProvider {

        [SerializeField] private Transform _center;
        [SerializeField] private float _weightMul = 1f;
        [SerializeField] private AnimationCurve _weightCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeField] [Range(0f, 1f)] private float _fallOff = 1f;
        [SerializeField] [Min(0f)] private float _innerRadius = 1f;
        [SerializeField] [Min(0f)] private float _outerRadius = 2f;

        public override float GetWeight(Vector3 position) {
            return ConvertLinearWeight(GetLinearWeight(position));
        }

        private float GetLinearWeight(Vector3 position) {
            if (_fallOff <= 0f) {
                return 1f;
            }
            
            var center = _center.position;
            if ((position - center).sqrMagnitude <= _innerRadius * _innerRadius) {
                return 1f;
            }

            if (_outerRadius - _innerRadius <= 0f) {
                return 1f - _fallOff;
            }
            
            float x = ((center - position).magnitude - _innerRadius) / (_outerRadius - _innerRadius);
            return Mathf.Clamp01(1f + 2f * _fallOff * (1f / (x + 1f) - 1f));
        }

        private float ConvertLinearWeight(float linearWeight) {
            return _weightCurve.Evaluate(linearWeight) * _weightMul;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        [VisibleIf(nameof(_showDebugInfo))]
        [SerializeField] private float _testPoint = 1f;
        
        private void Reset() {
            _center = transform;
        }
        
        private void OnValidate() {
            if (_outerRadius < _innerRadius) _outerRadius = _innerRadius;
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo || _center == null) return;

            _center.GetPositionAndRotation(out var position, out var rotation);
            
            DebugExt.DrawLabel(position + rotation * Vector3.up * 0.12f, $"W = {ConvertLinearWeight(1f):0.000}\nLin = {1f:0.000}");
            
            if (_fallOff <= 0f) return;

            var pIn = position + rotation * Vector3.forward * _innerRadius;
            float w = ConvertLinearWeight(1f);
            DebugExt.DrawSphere(position, _innerRadius, Color.white, gizmo: true);
            DebugExt.DrawPointer(pIn, Color.white, 0.03f, gizmo: true);
            DebugExt.DrawLabel(pIn + rotation * Vector3.right * 0.12f, $"W = {w:0.000}\nLin = {1f:0.000}", color: Color.white);
            DebugExt.DrawLine(pIn, position, Color.white, gizmo: true);
            
            var pOut = position + rotation * Vector3.forward * _outerRadius;
            w = GetWeight(pOut);
            DebugExt.DrawSphere(position, _outerRadius, Color.yellow, gizmo: true);
            DebugExt.DrawPointer(pOut, Color.yellow, 0.03f, gizmo: true);
            DebugExt.DrawLabel(pOut - rotation * Vector3.right * 0.12f, $"W = {w:0.000}\nLin = {GetLinearWeight(pOut):0.000}", color: Color.yellow);
            DebugExt.DrawLine(pOut, pIn, Color.yellow, gizmo: true);
            
            var pFar = position + rotation * Vector3.forward * _testPoint;
            w = GetWeight(pFar);
            DebugExt.DrawPointer(pFar, Color.cyan, 0.03f, gizmo: true);
            DebugExt.DrawLabel(pFar + rotation * Vector3.forward * 0.12f, $"W = {w:0.000}\nLin = {GetLinearWeight(pFar):0.000}", color: Color.cyan);
            if (_testPoint > _outerRadius) DebugExt.DrawLine(pFar, pOut, Color.cyan, gizmo: true);
        }
#endif
    }
    
}