using System;
using MisterGames.Scenes.Core;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public struct SceneTransactionLoad : ISceneTransaction {

        public SceneReference scene;
        public bool makeActive;

        public void Perform(SceneLoader sceneLoader) {
            sceneLoader.LoadScene(scene.scene, makeActive);
        }
    }

}
