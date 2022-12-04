using System;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public struct SceneTransactions : ISceneTransaction {

        [SerializeReference] [SubclassSelector]
        public ISceneTransaction[] operations;

        public async UniTask Perform(SceneLoader sceneLoader) {
            var tasks = new UniTask[operations.Length];
            for (int i = 0; i < operations.Length; i++) {
                tasks[i] = operations[i].Perform(sceneLoader);
            }
            await UniTask.WhenAll(tasks);
        }
    }

}
