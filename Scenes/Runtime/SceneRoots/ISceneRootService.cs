using System;

namespace MisterGames.Scenes.SceneRoots {
    
    public interface ISceneRootService {
        
        event Action<string, bool> OnSceneRootsEnableStateChanged;  
        
        void Register(ISceneRoot sceneRoot, string sceneName);
        void Unregister(ISceneRoot sceneRoot);

        bool HasSceneRootState(string sceneName, out bool enabled);
        void SetSceneRootEnabled(string sceneName, bool enabled);
    }
    
}