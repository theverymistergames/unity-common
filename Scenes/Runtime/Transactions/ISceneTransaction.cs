using Cysharp.Threading.Tasks;

namespace MisterGames.Scenes.Transactions {

    public interface ISceneTransaction {
        UniTask Commit();
    }

}
