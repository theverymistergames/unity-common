using System;
using System.Collections.Generic;
using MisterGames.Interact.Cursors;
using MisterGames.Interact.Detectables;
using UnityEngine;

namespace MisterGames.Interact.Interactives {

    public interface IInteractiveUser {

        event Action<IInteractive> OnDetected;
        event Action<IInteractive> OnLost;

        event Action<IInteractive> OnCandidateAdded;
        event Action<IInteractive> OnCandidateRemoved;

        event Action<IInteractive> OnStartInteract;
        event Action<IInteractive> OnStopInteract;

        IReadOnlyCollection<IInteractive> Interactives { get; }
        IReadOnlyCollection<IInteractive> Candidates { get; }
        IDetector Detector { get; }
        Transform Transform { get; }
        Transform ViewOrigin { get; }
        GameObject Root { get; }

        bool IsInDirectView(IInteractive interactive, out float distance);
        bool IsDetected(IInteractive interactive);
        bool IsCandidate(IInteractive interactive);
        bool IsInteractingWith(IInteractive interactive);

        bool TryStartInteract(IInteractive interactive);
        bool TryStopInteract(IInteractive interactive);
        bool ForceStopInteract(IInteractive interactive);
        void ForceStopInteractAll();

        void Block(object source, bool block);

        bool TryGetCursorHost(out ICursorHost host);
        void SetCursorHostOverride(object source, ICursorHost host);
        void ResetCursorHostOverride(object source);
    }

}
