using MisterGames.Scenes.Core;
using MisterGames.Tick.Jobs;

namespace MisterGames.Scenes.Transactions {

    public interface ISceneTransaction {
        ReadOnlyJob Perform(SceneLoader sceneLoader);
    }

}
