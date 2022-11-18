using System;
using MisterGames.Common.Attributes;
using MisterGames.Scenes.Core;
using MisterGames.Tick.Jobs;
using UnityEngine;

namespace MisterGames.Scenes.Transactions {

    [Serializable]
    public struct SceneTransactions : ISceneTransaction {

        [SerializeReference] [SubclassSelector]
        public ISceneTransaction[] operations;

        public IJobReadOnly Perform(SceneLoader sceneLoader) {
            var jobObserver = new JobObserver();

            for (int i = 0; i < operations.Length; i++) {
                jobObserver.Observe(operations[i].Perform(sceneLoader));
            }

            return jobObserver;
        }
    }

}
