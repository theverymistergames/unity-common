using System;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public sealed class SceneTransactions : ISceneTransaction {

        [SerializeReference] [SubclassSelector]
        public ISceneTransaction[] operations;

        public async UniTask Commit() {
            var tasks = new UniTask[operations.Length];

            for (int i = 0; i < operations.Length; i++) {
                tasks[i] = operations[i].Commit();
            }

            await UniTask.WhenAll(tasks);
        }
    }

}
