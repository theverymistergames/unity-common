using UnityEngine;

namespace MisterGames.Scenes.Loading {
    
    public interface ILoadingService {
    
        void ShowLoadingScreen(bool show);

        void RegisterLoadingScreenRoot(GameObject root);
    }
    
}