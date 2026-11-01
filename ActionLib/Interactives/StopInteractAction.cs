using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Interact.Interactives;

namespace MisterGames.ActionLib.Interactives {
    
    [Serializable]
    public sealed class StopInteractAction : IActorAction {

        public Interactive interactive;
        public bool force = true;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (!context.TryGetComponent(out IInteractiveUser user)) return default;

            if (interactive == null) {
                if (force) user.ForceStopInteractAll();
                return default;
            }

            if (force) user.ForceStopInteract(interactive);
            else user.TryStopInteract(interactive);

            return default;
        }
        
    }
    
}
