using MisterGames.Common.Collisions;
using MisterGames.Common.Collisions.Utils;
using MisterGames.Common.Routines;
using UnityEngine;

namespace MisterGames.Scenes.Core {

    public sealed class LoadSceneOnTrigger : MonoBehaviour {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private SceneReference _scene;
        [SerializeField] private LayerMask _layerMask;
        
        [SerializeField] [Min(0f)] private float _ignoreTriggerAfterSceneStartDelay = 1f;
        [SerializeField] [Min(0f)] private float _loadDelay = 0f;

        private readonly SingleJobHandler _handler = new SingleJobHandler();
        private IAsyncTaskReadOnly _currentLoading;
        private float _startTime;
        private bool _exitedOnce;

        private void OnDestroy() {
            _handler.Stop();
        }

        private void OnDisable() {
            _handler.Stop();
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
            Jobs.Do(_timeDomain.Delay(_loadDelay))
                .Then(Load)
                .Then(_timeDomain.EachFrameWhile(IsLoading))
                .StartFrom(_handler);
        }
        
        private void Load() {
            _currentLoading = SceneLoader.LoadScene(_scene.scene);
        }

        private bool IsLoading() {
            return _currentLoading is { IsDone: false };
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
