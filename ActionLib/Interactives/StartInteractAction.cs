using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Interact.Detectables;
using MisterGames.Interact.Interactives;

namespace MisterGames.ActionLib.Interactives {
    
    [Serializable]
    public sealed class StartInteractAction : IActorAction {

        public Interactive interactive;
        public Detectable detectable;
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (context.TryGetComponent(out IInteractiveUser user)) {
                if (detectable != null) user.Detector.ForceDetect(detectable);
                user.TryStartInteract(interactive);
            }

            return default;
        }
        
    }
    
}