using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    public interface IInteractive {

        event Action<IInteractiveUser> OnDetectedBy;
        event Action<IInteractiveUser> OnLostBy;

        event Action<IInteractiveUser> OnStartInteract;
        event Action<IInteractiveUser> OnStopInteract;

        IReadOnlyCollection<IInteractiveUser> Users { get; }
        Transform Transform { get; }

        bool IsInteractingWith(IInteractiveUser user);
        bool TryGetInteractionStartTime(IInteractiveUser user, out int startTime);

        bool IsReadyToStartInteractWith(IInteractiveUser user);
        bool IsAllowedToStartInteractWith(IInteractiveUser user);
        bool IsAllowedToContinueInteractWith(IInteractiveUser user);

        void NotifyDetectedBy(IInteractiveUser user);
        void NotifyLostBy(IInteractiveUser user);

        void NotifyStartedInteractWith(IInteractiveUser user);
        void NotifyStoppedInteractWith(IInteractiveUser user);

        void ForceStopInteractWithAllUsers();
    }

}
