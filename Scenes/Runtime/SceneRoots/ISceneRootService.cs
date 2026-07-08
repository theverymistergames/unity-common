namespace MisterGames.Scenes.SceneRoots {
    
    public interface ISceneRootService {
        
        public delegate void OnSceneRootsStateCallback(string sceneName, bool enabled);
        event OnSceneRootsStateCallback OnSceneRootsEnableStateChanged;  
        
        void Register(ISceneRoot sceneRoot, string sceneName);
        void Unregister(ISceneRoot sceneRoot);

        bool HasSceneRootState(string sceneName, out bool enabled);
        void SetSceneRootEnabled(string sceneName, bool enabled);
    }
    
}