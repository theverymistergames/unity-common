using System;
using Cysharp.Threading.Tasks;
using MisterGames.Scenes.Core;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public struct SceneTransactionLoad : ISceneTransaction {

        public SceneReference scene;
        public bool makeActive;

        public async UniTask Perform(SceneLoader sceneLoader) {
            await sceneLoader.LoadScene(scene.scene, makeActive);
        }
    }

}
