using MisterGames.Common.Layers;
using MisterGames.Scenes.Core;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;
using MisterGames.Tick.Utils;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    public sealed class SceneTransactionTrigger : MonoBehaviour {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private LayerMask _layerMask;

        [SerializeField] [Min(0f)] private float _ignoreTriggerAfterSceneStartDelay = 1f;
        [SerializeField] [Min(0f)] private float _loadDelay = 0f;

        [SerializeField] private SceneTransactions _sceneTransactions;

        private IJob _loadSceneJob;
        private float _startTime;
        private bool _exitedOnce;

        private void OnDestroy() {
            _loadSceneJob?.Stop();
        }

        private void OnDisable() {
            _loadSceneJob?.Stop();
        }

        private void Start() {
            _startTime = Time.realtimeSinceStartup;
        }

        private void OnTriggerEnter(Collider other) {
            if (!enabled) return;
            if (!CanTriggerByStartTime() || !CanTriggerByFilter(other.gameObject)) return;
            
            StartLoad();
        }

        private void OnTriggerExit(Collider other) {
            if (!enabled) return;
            if (!CanTriggerByFilter(other.gameObject)) return;
            
            _exitedOnce = true;
        }

        private void StartLoad() {
            _loadSceneJob?.Stop();

            _loadSceneJob = JobSequence.Create()
                .Delay(_loadDelay)
                .WaitCompletion(SceneLoader.Instance.CommitTransaction(_sceneTransactions))
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
