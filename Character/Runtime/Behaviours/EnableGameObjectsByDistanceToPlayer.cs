using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Character.Core;
using MisterGames.Common;
using UnityEngine;

namespace MisterGames.Character.Behaviours {
    
    public sealed class EnableGameObjectsByDistanceToPlayer : MonoBehaviour {

        [SerializeField] private Transform _center;
        [SerializeField] private GameObject[] _gameObjects;
        [SerializeField] [Min(0f)] private float _maxDistance = 5f;
        [SerializeField] [Min(0f)] private float _checkPeriodInRange = 0.1f;
        [SerializeField] [Min(0f)] private float _checkPeriodOutOfRange = 0.3f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private byte _trackId;
        
        private void Reset() {
            var go = gameObject;
            
            _center = go.transform;
            _gameObjects = new[] { go };
        }

        private void Start() {
            StartTrackingDistance(CharacterSystem.Instance.GetCharacter(), destroyCancellationToken).Forget();
        }

        private async UniTask StartTrackingDistance(IActor actor, CancellationToken cancellationToken) {
            byte id = ++_trackId;
            
            if (actor == null) return;

            while (!cancellationToken.IsCancellationRequested && id == _trackId) {
                var characterPos = actor.Transform.position;
                bool inRange = Vector3.SqrMagnitude(characterPos - transform.position) <= _maxDistance;
                float checkPeriod = inRange ? _checkPeriodInRange : _checkPeriodOutOfRange;

                for (int i = 0; i < _gameObjects.Length; i++) {
                    _gameObjects[i].SetActive(inRange);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(checkPeriod), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!_showDebugInfo || _center == null) return;
            
            DebugExt.DrawSphere(_center.position, _maxDistance, Color.green, gizmo: true);
        }
#endif        
    }
    
}