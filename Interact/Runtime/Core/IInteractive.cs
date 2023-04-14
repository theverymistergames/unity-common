using System;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public interface IInteractive {
        event Action<IInteractiveUser> OnDetectedByUser;
        event Action<IInteractiveUser> OnLostByUser;

        event Action<IInteractiveUser, Vector3> OnStartInteract;
        event Action<IInteractiveUser> OnStopInteract;



        IInteractiveUser User { get; }
        Vector3 Position { get; }
        bool IsInteracting { get; }
        bool IsDetected { get; }

        void DetectByUser(IInteractiveUser user);
        void LoseByUser(IInteractiveUser user);

        void StartInteractWithUser(IInteractiveUser user, Vector3 hitPoint);
        void StopInteractWithUser(IInteractiveUser user);
    }

}
