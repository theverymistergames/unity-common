using System;
using MisterGames.Scenes.Core;
using MisterGames.Tick.Jobs;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public struct SceneTransactionLoad : ISceneTransaction {

        public SceneReference scene;
        public bool makeActive;

        public ReadOnlyJob Perform(SceneLoader sceneLoader) {
            return sceneLoader.LoadScene(scene.scene, makeActive);
        }
    }

}
