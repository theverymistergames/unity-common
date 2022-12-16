using System;
using Cysharp.Threading.Tasks;
using MisterGames.Scenes.Core;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public sealed class SceneTransactionUnload : ISceneTransaction {

        public SceneReference scene;

        public async UniTask Commit() {
            await SceneLoader.UnloadSceneAsync(scene.scene);
        }
    }

}
