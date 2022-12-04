using System;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Layers;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    public sealed class SceneTransactionTrigger : MonoBehaviour {

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] [Min(0f)] private float _ignoreTriggerAfterSceneStartDelay = 1f;
        [SerializeField] [Min(0f)] private float _loadDelay = 0f;

        [SerializeField] private SceneTransactions _sceneTransactions;

        private float _startTime;
        private bool _exitedOnce;

        private void Start() {
            _startTime = Time.realtimeSinceStartup;
        }

        private void OnTriggerEnter(Collider other) {
            if (!enabled) return;
            if (!CanTriggerByStartTime() || !CanTriggerByFilter(other.gameObject)) return;
            
            CommitSceneTransaction().Forget();
        }

        private void OnTriggerExit(Collider other) {
            if (!enabled) return;
            if (!CanTriggerByFilter(other.gameObject)) return;
            
            _exitedOnce = true;
        }

        private async UniTaskVoid CommitSceneTransaction() {
            await UniTask.Delay(TimeSpan.FromSeconds(_loadDelay));
            SceneLoader.Instance.CommitTransaction(_sceneTransactions);
        }

        private bool CanTriggerByFilter(GameObject go) {
            return _layerMask.Contains(go.layer);
        }

        private bool CanTriggerByStartTime() {
            if (_exitedOnce) return true;
            float timeSinceStartup = Time.realtimeSinceStartup;
            float timeSinceStartElapsed = timeSinceStartup - _startTime;
            return timeSinceStartElapsed > _ignoreTriggerAfterSceneStartDelay;
        }
    }
    
}
