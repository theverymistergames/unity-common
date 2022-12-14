using MisterGames.Common.Layers;
using MisterGames.Scenes.Core;
using MisterGames.Tick.Jobs;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    public sealed class SceneTransactionTrigger : MonoBehaviour {

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] [Min(0f)] private float _ignoreTriggerAfterSceneStartDelay = 1f;
        [SerializeField] [Min(0f)] private float _loadDelay = 0f;

        [SerializeField] private SceneTransactions _sceneTransactions;

        private Job _loadJob;

        private float _startTime;
        private bool _exitedOnce;

        private void OnDestroy() {
            _loadJob.Dispose();
        }

        private void Start() {
            _startTime = Time.realtimeSinceStartup;
        }

        private void OnTriggerEnter(Collider other) {
            if (!enabled) return;
            if (!CanTriggerByStartTime() || !CanTriggerByFilter(other.gameObject)) return;
            
            CommitSceneTransaction();
        }

        private void OnTriggerExit(Collider other) {
            if (!enabled) return;
            if (!CanTriggerByFilter(other.gameObject)) return;
            
            _exitedOnce = true;
        }

        private void CommitSceneTransaction() {
            _loadJob.Dispose();

            _loadJob = JobSequence.Create()
                .Delay(_loadDelay)
                .Wait(SceneLoader.Instance.CommitTransaction(_sceneTransactions))
                .Push()
                .Start();
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
