using System;
using MisterGames.Common.Conditions;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionGroup :
        ConditionGroup<IDetectCondition, (IDetector, IDetectable)>,
        IDetectCondition { }

}
