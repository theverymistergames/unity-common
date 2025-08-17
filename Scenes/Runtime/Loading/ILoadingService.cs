namespace MisterGames.Scenes.Loading {
    
    public interface ILoadingService {
    
        string LoadingScene { get; }

        void ShowLoadingScreen(bool show);
    }
    
}