using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Core;

namespace MisterGames.ActionLib.Dialogues {
    
    [Serializable]
    public sealed class ClearDialoguePrintersAction : IActorAction {

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
			Services.Get<IDialogueService>()?.ClearAllPrinters();
            return UniTask.CompletedTask;
        }
    }
    
}