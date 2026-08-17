using System;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Volumes;
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
        [SerializeField] private PositionWeightProvider _positionWeightProvider;

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
                    CustomGravity.Main.AddGravitySource(this);
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
                    CustomGravity.Main.RemoveGravitySource(this);
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
            return _positionWeightProvider.GetWeight(position).weight;
        }

        private float GetFullMagnitude() {
            return _gravityMagnitude * (_useScaleZAsMultiplier ? _source.localScale.z : 1f);
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        [VisibleIf(nameof(_showDebugInfo))]
        [SerializeField] private float _testPoint = 1f;

        private void Reset() {
            _source = transform;
        }

        private void OnDrawGizmos() {
            if (!_showDebugInfo || _source == null) return;

            _source.GetPositionAndRotation(out var position, out var rotation);
            
            DebugExt.DrawLabel(position + rotation * Vector3.up * 0.12f, $"G = {_weightMul * GetFullMagnitude():0.000}");
            
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
        }
#endif
    }
    
}