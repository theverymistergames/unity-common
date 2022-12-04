using System;
using MisterGames.Scenes.Core;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public struct SceneTransactionUnload : ISceneTransaction {

        public SceneReference scene;

        public void Perform(SceneLoader sceneLoader) {
            sceneLoader.UnloadScene(scene.scene);
        }
    }

}
