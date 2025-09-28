using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Scenes.Background {
    
    [DefaultExecutionOrder(-110_000)]
    public sealed class BackgroundSceneServiceRunner : MonoBehaviour {
        
        [SerializeField] private BackgroundSceneService _backgroundSceneService;
        
        private void Awake() {
            _backgroundSceneService.Initialize();
            Services.Register<IBackgroundSceneService>(_backgroundSceneService);
        }

        private void OnDestroy() {
            Services.Unregister(_backgroundSceneService);
            _backgroundSceneService.Dispose();
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (!Application.isPlaying) return;
            
            _backgroundSceneService?.OnValidate();
        }  
#endif
    }
    
}