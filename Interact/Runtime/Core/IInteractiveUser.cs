using System;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public interface IInteractiveUser {
        event Action<IInteractive> OnInteractiveDetected;
        event Action<IInteractive> OnInteractiveLost;

        event Action<IInteractive, Vector3> OnStartInteract;
        event Action<IInteractive> OnStopInteract;

        GameObject GameObject { get; }
        Vector3 Position { get; }
        IInteractive PossibleInteractive { get; }
        bool IsInteracting { get; }

        void StartInteract();
        void StopInteract();
    }

}
