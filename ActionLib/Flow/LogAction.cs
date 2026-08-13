using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.Flow {

    [Serializable]
    public sealed class LogAction : IActorAction {
        
        public string text = "log";
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            Debug.Log($"LogAction.Apply: f {UnityEngine.Time.frameCount}, {text}");
            return default;
        }
    }
    
}