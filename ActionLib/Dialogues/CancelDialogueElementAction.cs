using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Core;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class CancelDialogueElementAction : IActorAction {

        public DialogueCancelMode cancelMode;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
			Services.Get<IDialogueService>()?.CancelCurrentElementPrinting(cancelMode);
            return UniTask.CompletedTask;
        }
    }
    
}