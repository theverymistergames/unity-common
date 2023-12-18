using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    public interface IInteractiveUser {

        event Action<IInteractive> OnDetected;
        event Action<IInteractive> OnLost;

        event Action<IInteractive> OnStartInteract;
        event Action<IInteractive> OnStopInteract;

        IReadOnlyCollection<IInteractive> Interactives { get; }
        Transform Transform { get; }
        GameObject Root { get; }

        bool IsInDirectView(IInteractive interactive, out float distance);
        bool IsDetected(IInteractive interactive);
        bool IsInteractingWith(IInteractive interactive);

        bool TryStartInteract(IInteractive interactive);
        bool TryStopInteract(IInteractive interactive);
        void ForceStopInteractAll();
    }

}
