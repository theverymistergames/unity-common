using UnityEngine;

namespace MisterGames.Scenes.Background {
    
    [DefaultExecutionOrder(-110_000)]
    public sealed class BackgroundSceneServiceRunner : MonoBehaviour {
        
        [SerializeField] private BackgroundSceneService _backgroundSceneService;
        
        private void Awake() {
            _backgroundSceneService.Initialize();
        }

        private void OnDestroy() {
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