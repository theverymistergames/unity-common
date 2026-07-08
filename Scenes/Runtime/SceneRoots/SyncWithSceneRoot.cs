using MisterGames.Common.GameObjects;
using MisterGames.Common.Service;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.SceneRoots {
    
    public sealed class SyncWithSceneRoot : MonoBehaviour {
        
        [SerializeField] private SceneReference scene;
        [SerializeField] private Object[] enableOnSceneRootEnabled;
        [SerializeField] private Object[] disableOnSceneRootEnabled;
        
        private void Awake() {
            if (Services.TryGet(out ISceneRootService service)) {
                service.OnSceneRootsEnableStateChanged += OnSceneRootsEnableStateChanged;
                SetState(IsEnabled());
            }
        }

        private void OnDestroy() {
            if (Services.TryGet(out ISceneRootService service)) {
                service.OnSceneRootsEnableStateChanged -= OnSceneRootsEnableStateChanged;
            }
        }
        
        private void OnSceneRootsEnableStateChanged(string sceneName, bool enabled) {
            SetState(IsEnabled());
        }

        private bool IsEnabled() {
            return !Services.TryGet(out ISceneRootService service) ||
                   !service.HasSceneRootState(scene.scene, out bool enabled) || 
                   enabled;
        }
        
        private void SetState(bool enabled) {
            enableOnSceneRootEnabled.SetEnabled(enabled);
            disableOnSceneRootEnabled.SetEnabled(!enabled);
        }
    }
    
}