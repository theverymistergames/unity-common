namespace MisterGames.Scenes.Loading {
    
    public interface ILoadingService {
    
        string LoadingScene { get; }

        void Initialize();
        void ShowLoadingScreen(bool show);
    }
    
}