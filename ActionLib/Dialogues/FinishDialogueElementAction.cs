using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Core;
using UnityEngine;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class FinishDialogueElementAction : IActorAction {

        [Min(-1f)] public float symbolDelay = -1f;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
			Services.Get<IDialogueService>()?.FinishLastPrinting(symbolDelay);
            return UniTask.CompletedTask;
        }
    }
    
}