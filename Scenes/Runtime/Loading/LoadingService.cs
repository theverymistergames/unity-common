using UnityEngine;

namespace MisterGames.Scenes.Loading {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class LoadingService : MonoBehaviour, ILoadingService {
        
        public static ILoadingService Instance { get; private set; }
        
        private bool _showLoadingScreen;
        private GameObject _loadingScreenRoot;
        
        private void Awake() {
            Instance = this;
        }

        public void ShowLoadingScreen(bool show) {
            _showLoadingScreen = show;
            if (_loadingScreenRoot != null) _loadingScreenRoot.SetActive(show);
        }

        public void RegisterLoadingScreenRoot(GameObject root) {
            _loadingScreenRoot = root;
            _loadingScreenRoot.SetActive(_showLoadingScreen);
        }
    }
    
}