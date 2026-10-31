using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Character.Core;
using MisterGames.Character.View;
using MisterGames.Common;
using MisterGames.Common.Async;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Behaviours {
    
    public sealed class EnableGameObjectsByDistanceToPlayer : MonoBehaviour {

        [SerializeField] private Transform _center;
        [SerializeField] private GameObject[] _gameObjects;
        [SerializeField] [Min(0f)] private float _maxDistance = 5f;
        [SerializeField] [Min(0f)] private float _checkPeriodInRange = 0.1f;
        [SerializeField] [Min(0f)] private float _checkPeriodOutOfRange = 0.3f;

        [Header("Frustum")]
        [SerializeField] private FrustumMode _checkFrustum;
        [SerializeField] [Min(0f)] private float _objectRadius = 1f;

        private enum FrustumMode {
            Skip,
            EnableOnScreen,
            EnableOffScreen,
        }

        private CancellationTokenSource _enableCts;
        private readonly Plane[] _frustumPlanes = new Plane[6];
        private byte _trackId;

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            StartTrackingDistance(CharacterSpawner.Instance.GetCharacter(), _enableCts.Token).Forget();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
        }

        private async UniTask StartTrackingDistance(IActor actor, CancellationToken cancellationToken) {
            byte id = _trackId.IncrementUncheckedRef();
            
            if (actor == null) return;

            actor.TryGetComponent(out CameraContainer cameraContainer);

            while (!cancellationToken.IsCancellationRequested && id == _trackId) {
                var characterPos = actor.Transform.position;
                bool inRange = Vector3.SqrMagnitude(characterPos - _center.position) <= _maxDistance * _maxDistance;
                float checkPeriod = inRange ? _checkPeriodInRange : _checkPeriodOutOfRange;
                bool isActive = inRange && 
                                (_checkFrustum == FrustumMode.Skip || 
                                 (_checkFrustum == FrustumMode.EnableOnScreen) == IsInFrustum(cameraContainer));

                for (int i = 0; i < _gameObjects.Length; i++) {
                    _gameObjects[i].SetActive(isActive);
                }

                await AsyncExt.Delay(TimeSpan.FromSeconds(checkPeriod), cancellationToken);
            }
        }

        private bool IsInFrustum(CameraContainer cameraContainer) {
            var camera = cameraContainer == null ? null : cameraContainer.Camera;
            if (camera == null) return true;

            GeometryUtility.CalculateFrustumPlanes(camera, _frustumPlanes);
            var center = _center.position;

            for (int i = 0; i < _frustumPlanes.Length; i++) {
                if (_frustumPlanes[i].GetDistanceToPoint(center) < -_objectRadius) return false;
            }

            return true;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void Reset() {
            _center = gameObject.transform;
            _gameObjects = new[] { gameObject };
        }
        
        private void OnDrawGizmos() {
            if (!_showDebugInfo || _center == null) return;
            
            DebugExt.DrawSphere(_center.position, _maxDistance, Color.green, gizmo: true);
            if (_checkFrustum != FrustumMode.Skip) DebugExt.DrawSphere(_center.position, _objectRadius, Color.yellow, gizmo: true);
        }
#endif        
    }
    
}