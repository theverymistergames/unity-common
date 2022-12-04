using System;
using Cysharp.Threading.Tasks;
using MisterGames.Scenes.Core;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public struct SceneTransactionUnload : ISceneTransaction {

        public SceneReference scene;

        public async UniTask Perform(SceneLoader sceneLoader) {
            await sceneLoader.UnloadScene(scene.scene);
        }
    }

}
