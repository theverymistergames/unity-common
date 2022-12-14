using System;
using MisterGames.Scenes.Core;
using MisterGames.Tick.Jobs;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public struct SceneTransactionUnload : ISceneTransaction {

        public SceneReference scene;

        public ReadOnlyJob Perform(SceneLoader sceneLoader) {
            return sceneLoader.UnloadScene(scene.scene);
        }
    }

}
