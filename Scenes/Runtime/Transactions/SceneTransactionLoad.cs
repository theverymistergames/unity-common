using System;
using Cysharp.Threading.Tasks;
using MisterGames.Scenes.Core;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public sealed class SceneTransactionLoad : ISceneTransaction {

        public SceneReference scene;
        public bool makeActive;

        public async UniTask Commit() {
            await SceneLoader.LoadSceneAsync(scene.scene, makeActive);
        }
    }

}
