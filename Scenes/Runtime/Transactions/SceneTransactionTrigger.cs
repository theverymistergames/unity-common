using MisterGames.Common.Layers;
using MisterGames.Scenes.Core;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    public sealed class SceneTransactionTrigger : MonoBehaviour {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] [Min(0f)] private float _ignoreTriggerAfterSceneStartDelay = 1f;
        [SerializeField] [Min(0f)] private float _loadDelay = 0f;

        [SerializeField] private SceneTransactions _sceneTransactions;

        private float _startTime;
        private bool _exitedOnce;

        private IJob _loadJob;

        private void OnDestroy() {
            _loadJob?.Stop();
        }

        private void OnDisable() {
            _loadJob?.Stop();
        }

        private void Start() {
            _startTime = Time.realtimeSinceStartup;
        }

        private void OnTriggerEnter(Collider other) {
            if (!enabled) return;
            if (!CanTriggerByStartTime() || !CanTriggerByFilter(other.gameObject)) return;
            
            RestartLoadJob();
        }

        private void OnTriggerExit(Collider other) {
            if (!enabled) return;
            if (!CanTriggerByFilter(other.gameObject)) return;
            
            _exitedOnce = true;
        }

        private void RestartLoadJob() {
            _loadJob?.Stop();

            _loadJob = JobSequence.Create()
                .Delay(_loadDelay)
                .Wait(SceneLoader.Instance.CommitTransaction(_sceneTransactions))
                .RunFrom(_timeDomain.Source);
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
