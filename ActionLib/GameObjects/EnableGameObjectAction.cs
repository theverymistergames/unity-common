using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {

    [Serializable]
    public sealed class EnableGameObjectAction : IActorAction {

        public bool enabled;
        [Min(0f)] public float delay;
        public GameObject[] gameObjects;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (delay > 0f) {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();
            }
            
            if (cancellationToken.IsCancellationRequested) return;
            
            for (int i = 0; i < gameObjects.Length; i++) {
                gameObjects[i].SetActive(enabled);
            }
        }
    }
    
}