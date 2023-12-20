using System;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class DetectConditionInDirectView : IDetectCondition {

        public bool shouldBeInDirectView;

        public bool IsMatch(IDetector detector, IDetectable detectable) {
            return shouldBeInDirectView == detector.IsInDirectView(detectable, out _);
        }
    }

}
