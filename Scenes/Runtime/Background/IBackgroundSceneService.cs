namespace MisterGames.Scenes.Background {
    
    public interface IBackgroundSceneService {

        void BindBackgroundScene(object source, string sceneName);

        void UnbindBackgroundScene(object source, string sceneName);
    }
    
}