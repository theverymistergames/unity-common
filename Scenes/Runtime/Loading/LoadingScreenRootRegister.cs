using UnityEngine;

namespace MisterGames.Scenes.Loading {
    
    public sealed class LoadingScreenRootRegister : MonoBehaviour {
        
        private void Awake() {
            LoadingService.Instance.RegisterLoadingScreenRoot(gameObject);
        }
    }
    
}