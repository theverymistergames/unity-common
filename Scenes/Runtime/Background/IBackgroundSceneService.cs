namespace MisterGames.Scenes.Background {
    
    public interface IBackgroundSceneService {

        void BindBackgroundScene(object source, string sceneName, bool makeActive = false);

        void UnbindBackgroundScene(object source, string sceneName);
    }
    
}