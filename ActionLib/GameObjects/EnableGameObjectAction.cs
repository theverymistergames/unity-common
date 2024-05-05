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
        public GameObject[] gameObjects;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            for (int i = 0; i < gameObjects.Length; i++) {
                gameObjects[i].SetActive(enabled);
            }

            return default;
        }
    }
    
}