using System;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public interface IInteractiveUser {
        event Action<IInteractive> OnInteractiveDetected;
        event Action<IInteractive> OnInteractiveLost;

        event Action<IInteractive, Vector3> OnStartInteract;
        event Action<IInteractive> OnStopInteract;

        GameObject GameObject { get; }
        ITransformAdapter TransformAdapter { get; }
        IInteractive PossibleInteractive { get; }
        bool IsInteracting { get; }

        void StartInteract();
        void StopInteract();
    }

}
