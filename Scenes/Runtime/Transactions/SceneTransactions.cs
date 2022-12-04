using System;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Core;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public struct SceneTransactions : ISceneTransaction {

        [SerializeReference] [SubclassSelector]
        public ISceneTransaction[] operations;

        public void Perform(SceneLoader sceneLoader) {
            for (int i = 0; i < operations.Length; i++) {
                operations[i].Perform(sceneLoader);
            }
        }
    }

}
