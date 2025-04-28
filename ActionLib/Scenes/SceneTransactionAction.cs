using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Scenes.Transactions;

namespace MisterGames.ActionLib.Scenes {
    
    [Serializable]
    public sealed class SceneTransactionAction : IActorAction {
        
        public SceneTransaction[] sceneTransactions;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            for (int i = 0; i < sceneTransactions.Length; i++) {
                await sceneTransactions[i].Apply(cancellationToken);
            }
        }
    }
}