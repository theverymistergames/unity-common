using UnityEngine;

namespace MisterGames.Scenes.Loading {
    
    public sealed class LoadingScreenOverlayBlock : MonoBehaviour {
        
        private void OnEnable() { 
            LoadingService.Instance.BlockLoadingScreenOverlay(this, true);   
        }

        private void OnDisable() {
            LoadingService.Instance.BlockLoadingScreenOverlay(this, false);
        }
    }
    
}