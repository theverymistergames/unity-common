using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Scenes.SceneRoots {
    
    public sealed class SceneRoot : MonoBehaviour, ISceneRoot {
        
        [SerializeField] private GameObject _root;
        
        private void Awake() {
            Services.Get<ISceneRootService>()?.Register(this, _root.scene.name);
        }

        private void OnDestroy() {
            Services.Get<ISceneRootService>()?.Unregister(this);
        }

        public void SetEnabled(bool enabled) {
            _root.SetActive(enabled);
        }

#if UNITY_EDITOR
        private void Reset() {
            _root = gameObject;
        }
#endif
    }
    
}