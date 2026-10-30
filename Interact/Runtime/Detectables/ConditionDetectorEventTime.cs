using System;
using MisterGames.Common.Data;
using MisterGames.Common.Tick;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    [Serializable]
    public sealed class ConditionDetectorEventTime : IDetectCondition, IInteractCondition {

        public DetectorEvent detectorEvent;
        [Min(0f)] public float minTime;
        public Optional<float> maxTime;
        public bool matchIfNoEvent;

        public bool IsMatch((IDetector, IDetectable) context) {
            var (detector, detectable) = context;
            return IsMatch(detector, detectable);
        }

        public bool IsMatch((IInteractiveUser, IInteractive) context) {
            var (user, interactive) = context;
            return IsMatch(user?.Detector, interactive?.Detectable);
        }

        private bool IsMatch(IDetector detector, IDetectable detectable) {
            float time = detector?.GetDetectableLastTimeOf(detectorEvent, detectable) ?? -1f;
            if (time < 0f) return matchIfNoEvent;

            float elapsed = TimeSources.scaledTime - time;
            return elapsed >= minTime && (!maxTime.HasValue || elapsed <= maxTime.Value);
        }
    }

}
