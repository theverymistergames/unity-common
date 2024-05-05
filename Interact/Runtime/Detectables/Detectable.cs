using System;
using System.Collections.Generic;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    public sealed class Detectable : MonoBehaviour, IDetectable {

        [EmbeddedInspector]
        [SerializeField] private DetectStrategy _strategy;

        public event Action<IDetector> OnDetectedBy = delegate {  };
        public event Action<IDetector> OnLostBy = delegate {  };

        public IReadOnlyCollection<IDetector> Observers => _observersSet;
        public Transform Transform { get; private set; }

        private readonly HashSet<IDetector> _observersSet = new HashSet<IDetector>();
        private readonly List<IDetector> _observers = new List<IDetector>();

        private void Awake() {
            Transform = transform;
        }

        private void OnDisable() {
            ForceRemoveAllObservers();
        }

        public bool IsDetectedBy(IDetector detector) {
            return _observersSet.Contains(detector);
        }

        public bool IsAllowedToStartDetectBy(IDetector detector) {
            return enabled && _strategy.IsAllowedToStartDetection(detector, this);
        }

        public bool IsAllowedToContinueDetectBy(IDetector detector) {
            return enabled && _strategy.IsAllowedToContinueDetection(detector, this);
        }

        public void NotifyDetectedBy(IDetector detector) {
            if (_observersSet.Contains(detector)) return;

            _observersSet.Add(detector);
            _observers.Add(detector);

            OnDetectedBy.Invoke(detector);
        }

        public void NotifyLostBy(IDetector detector) {
            if (!_observersSet.Contains(detector)) return;

            _observersSet.Remove(detector);
            _observers.Remove(detector);

            OnLostBy.Invoke(detector);
        }

        public void ForceRemoveAllObservers() {
            _observersSet.Clear();

            for (int i = 0; i < _observers.Count; i++) {
                OnLostBy.Invoke(_observers[i]);
            }

            _observers.Clear();
        }

        public override string ToString() {
            return $"{nameof(Detectable)}({name}, observers count = {_observersSet.Count})";
        }

        [Header("Debug")]
        [SerializeField] private bool _debugDrawDetectable;

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!Application.isPlaying || !_debugDrawDetectable) return;

            DebugExt.DrawSphere(transform.position, 0.4f, Color.yellow, mode: DebugExt.DrawMode.Gizmo);
        }
#endif
    }

}
