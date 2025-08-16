using System.Threading;
using Cysharp.Threading.Tasks;

namespace MisterGames.Scenes.Core {
    
    public interface ISceneLoaderAction {

        UniTask Apply(CancellationToken cancellationToken);
        
    }
    
}