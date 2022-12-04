using Cysharp.Threading.Tasks;
using MisterGames.Scenes.Core;

namespace MisterGames.Scenes.Transactions {

    public interface ISceneTransaction {
        UniTask Perform(SceneLoader sceneLoader);
    }

}
