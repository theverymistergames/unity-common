using MisterGames.Character.Core;
using MisterGames.Common;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Behaviours {
    
    public sealed class EnableObjectsByDistanceToPlayer : MonoBehaviour, IUpdate {

        [SerializeField] private Transform _center;
        [SerializeField] private Object[] _objects;
        [SerializeField] [Min(0f)] private float _checkPeriodActive = 0.1f;
        [SerializeField] [Min(0f)] private float _checkPeriodInactive = 0.3f;

        [Header("Range")]
        [SerializeField] private DistanceMode _distanceMode = DistanceMode.EnableInRange;
        [SerializeField] [Min(0f)] private float _maxDistance = 5f;
        
        [Header("Frustum")]
        [SerializeField] private FrustumMode _frustumMode = FrustumMode.Skip;
        [SerializeField] [Min(0f)] private float _objectRadius = 1f;

        private enum DistanceMode {
            EnableInRange,
            EnableOffRange,
        }
        
        private enum FrustumMode {
            Skip,
            EnableOnScreen,
            EnableOffScreen,
        }

        private readonly Plane[] _frustumPlanes = new Plane[6];
        private Camera _camera;
        private float _checkTimer;

        private void OnEnable() {
            _checkTimer = 0f;
            TryFetchCamera(out _camera);
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.Update.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            _checkTimer -= dt;
            if (_checkTimer > 0f) return;

            bool inRange = false;
            bool inFrustum = false;

            if (_camera != null || TryFetchCamera(out _camera)) {
                var centerPos = _center.position;
                inRange = Vector3.SqrMagnitude(_camera.transform.position - centerPos) <= _maxDistance * _maxDistance;

                if (_frustumMode != FrustumMode.Skip) {
                    GeometryUtility.CalculateFrustumPlanes(_camera, _frustumPlanes);
                    inFrustum = IsInFrustum(_frustumPlanes, centerPos, _objectRadius);
                }
            }
            
            bool isActive = (_distanceMode == DistanceMode.EnableInRange) == inRange &&
                            
                            (_frustumMode == FrustumMode.Skip || 
                             (_frustumMode == FrustumMode.EnableOnScreen) == inFrustum);

            _checkTimer = isActive ? _checkPeriodActive : _checkPeriodInactive;
            
            for (int i = 0; i < _objects?.Length; i++) {
                _objects[i].SetEnabled(isActive);
            }
        }

        private static bool TryFetchCamera(out Camera camera) {
            if (CharacterSpawner.Instance.GetCharacter() is { } actor 
                && actor.GetComponent<Camera>() is { } cam) 
            {
                camera = cam;
                return true;
            }

            camera = null;
            return false;
        }

        private static bool IsInFrustum(Plane[] planes, Vector3 position, float radius) {
            for (int i = 0; i < planes.Length; i++) {
                if (planes[i].GetDistanceToPoint(position) < -radius) return false;
            }

            return true;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void Reset() {
            _center = gameObject.transform;
            _objects = new[] { (Object) gameObject };
        }
        
        private void OnDrawGizmos() {
            if (!_showDebugInfo || _center == null) return;
            
            DebugExt.DrawSphere(_center.position, _maxDistance, Color.green, gizmo: true);
            if (_frustumMode != FrustumMode.Skip) DebugExt.DrawSphere(_center.position, _objectRadius, Color.yellow, gizmo: true);
        }
#endif        
    }
    
}