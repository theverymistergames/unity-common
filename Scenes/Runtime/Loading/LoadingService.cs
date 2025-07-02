using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.Loading {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class LoadingService : MonoBehaviour, ILoadingService {
        
        [SerializeField] private bool _makeSceneActiveWhenLoading = true;
        
        public static ILoadingService Instance { get; private set; }
        
        private bool _showLoadingScreen;
        private GameObject _loadingScreenRoot;
        
        private void Awake() {
            Instance = this;
        }

        public void ShowLoadingScreen(bool show) {
            _showLoadingScreen = show;
            if (_loadingScreenRoot == null) return;
            
            _loadingScreenRoot.SetActive(show);
            
            if (_makeSceneActiveWhenLoading && show) {
                SceneLoader.SetActiveScene(_loadingScreenRoot.gameObject.scene.name);
            }
        }

        public void RegisterLoadingScreenRoot(GameObject root) {
            _loadingScreenRoot = root;
            _loadingScreenRoot.SetActive(_showLoadingScreen);
        }
    }
    
}