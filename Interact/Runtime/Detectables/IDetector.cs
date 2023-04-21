using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    public interface IDetector {

        event Action<IDetectable> OnDetected;
        event Action<IDetectable> OnLost;

        IReadOnlyCollection<IDetectable> Targets { get; }
        Transform Transform { get; }

        bool IsInDirectView(IDetectable detectable, out float distance);
        bool IsDetected(IDetectable detectable);

        void ForceDetect(IDetectable detectable);
        void ForceLose(IDetectable detectable);
        void ForceLoseAll();
    }

}
