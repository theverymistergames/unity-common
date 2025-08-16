namespace MisterGames.Scenes.Loading {
    
    public interface ILoadingService {
    
        string LoadingScene { get; }
        
        void ShowLoadingScreen(bool show);
        void BlockLoadingScreenOverlay(object source, bool block);

        void RegisterLoadingScreen(ILoadingScreen loadingScreen);
        void UnregisterLoadingScreen(ILoadingScreen loadingScreen);
    }
    
}