using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.ActionLib.GameObjects {
    
    [Serializable]
    public sealed class ReleaseToPoolAction : IActorAction {

        public GameObject[] gameObjects;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            for (int i = 0; i < gameObjects.Length; i++) {
                PrefabPool.Main.Release(gameObjects[i]);
            }

            return default;
        }
    }
    
}