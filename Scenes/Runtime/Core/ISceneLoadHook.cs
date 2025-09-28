using System.Threading;
using Cysharp.Threading.Tasks;

namespace MisterGames.Scenes.Core {
    
    public interface ISceneLoadHook {
        
        UniTask OnSceneLoadRequest(string sceneName, CancellationToken cancellationToken);
        
        UniTask OnSceneUnloadRequest(string sceneName, CancellationToken cancellationToken);
        
        bool CanUnloadScene(string sceneName);
    }
    
}