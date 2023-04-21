using System;
using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    public interface IDetectable {

        event Action<IDetector> OnDetectedBy;
        event Action<IDetector> OnLostBy;

        IReadOnlyCollection<IDetector> Observers { get; }
        Transform Transform { get; }

        bool IsDetectedBy(IDetector detector);

        bool IsAllowedToStartDetectBy(IDetector detector);
        bool IsAllowedToContinueDetectBy(IDetector detector);

        void NotifyDetectedBy(IDetector detector);
        void NotifyLostBy(IDetector detector);

        void ForceRemoveAllObservers();
    }

}
