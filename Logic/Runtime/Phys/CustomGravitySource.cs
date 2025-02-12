using System;
using MisterGames.Common;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class CustomGravitySource : MonoBehaviour, IGravitySource {

        [Header("Source")]
        [SerializeField] private Transform _source;
        [SerializeField] private Usage _usage;
        [SerializeField] private SourceMode _sourceMode;
        
        [Header("Force")]
        [SerializeField] private bool _useScaleZAsMultiplier;
        [SerializeField] private float _gravityMagnitude = 9.81f;
        
        [Header("Weight")]
        [SerializeField] private float _weightMul = 1f;
        [SerializeField] [Range(0f, 1f)] private float _fallOff;
        [SerializeField] [Min(0f)] private float _innerRadius = 1f;
        [SerializeField] [Min(0f)] private float _outerRadius = 2f;

        private enum Usage {
            AsGlobalGravitySource,
            AsLocalGravitySource,
        }

        private enum SourceMode {
            UseForwardAsDirection,
            UseAsGravityCenter,
        }

        public float GravityMagnitude { get => _gravityMagnitude; set => _gravityMagnitude = value; }

        private void OnEnable() {
            switch (_usage) {
                case Usage.AsGlobalGravitySource:
                    CustomGravity.AddGravitySource(this);
                    break;
                
                case Usage.AsLocalGravitySource:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDisable() {
            switch (_usage) {
                case Usage.AsGlobalGravitySource:
                    CustomGravity.RemoveGravitySource(this);
                    break;
                
                case Usage.AsLocalGravitySource:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Vector3 GetGravity(Vector3 position, out float weight) {
            weight = GetWeight(position);
            
            var dir = _sourceMode switch {
                SourceMode.UseForwardAsDirection => _source.forward,
                SourceMode.UseAsGravityCenter => position - _source.position,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (dir == Vector3.zero) return dir;
            
            return GetFullMagnitude() * weight * dir.normalized;
        }

        private float GetWeight(Vector3 position) {
            if (_fallOff <= 0f) return _weightMul;
            
            var center = _source.position;
            
            float sqrDistance = (position - center).sqrMagnitude;
            if (sqrDistance < _innerRadius * _innerRadius) return _weightMul;
            
            if (_outerRadius - _innerRadius <= 0f) return _weightMul * (1f - _fallOff);

            float x = ((center - position).magnitude - _innerRadius) / (_outerRadius - _innerRadius);
            return Mathf.Clamp01(1f + 2f * _fallOff * (1f / (x + 1f) - 1f)) * _weightMul;
        }

        private float GetFullMagnitude() {
            return _gravityMagnitude * (_useScaleZAsMultiplier ? _source.localScale.z : 1f);
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void Reset() {
            _source = transform;
        }

        private void OnValidate() {
            if (_outerRadius < _innerRadius) _outerRadius = _innerRadius;
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo || _source == null) return;

            _source.GetPositionAndRotation(out var position, out var rotation);
            
            DebugExt.DrawLabel(position + rotation * Vector3.up * 0.12f, $"G = {_weightMul * GetFullMagnitude():0.00}");
            
            switch (_sourceMode) {
                case SourceMode.UseForwardAsDirection:
                    DebugExt.DrawCircle(position, rotation * Quaternion.Euler(90f, 0f, 0f), 0.1f, Color.magenta, gizmo: true);        
                    DebugExt.DrawRay(position, rotation * Vector3.forward, Color.magenta, gizmo: true);
                    break;
                
                case SourceMode.UseAsGravityCenter:
                    DebugExt.DrawSphere(position, 0.1f, Color.magenta, gizmo: true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (_fallOff <= 0f) return;

            var pIn = position + rotation * Vector3.forward * _innerRadius;
            float w = GetWeight(pIn);
            DebugExt.DrawSphere(position, _innerRadius, Color.white, gizmo: true);
            DebugExt.DrawPointer(pIn, Color.white, 0.03f, gizmo: true);
            DebugExt.DrawLabel(pIn + rotation * Vector3.right * 0.12f, $"W = {w:0.00}\nG = {w * GetFullMagnitude():0.00}");
            DebugExt.DrawLine(pIn, position, Color.white, gizmo: true);
            
            var pOut = position + rotation * Vector3.forward * _outerRadius;
            w = GetWeight(pOut);
            DebugExt.DrawSphere(position, _outerRadius, Color.yellow, gizmo: true);
            DebugExt.DrawPointer(pOut, Color.yellow, 0.03f, gizmo: true);
            DebugExt.DrawLabel(pOut - rotation * Vector3.right * 0.12f, $"W = {w:0.00}\nG = {w * GetFullMagnitude():0.00}");
            DebugExt.DrawLine(pOut, pIn, Color.yellow, gizmo: true);
        }
#endif
    }
    
}