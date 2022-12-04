using MisterGames.Scenes.Core;

namespace MisterGames.Scenes.Transactions {

    public interface ISceneTransaction {
        void Perform(SceneLoader sceneLoader);
    }

}
