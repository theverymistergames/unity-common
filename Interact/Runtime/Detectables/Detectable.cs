using System;
using System.Collections.Generic;
using MisterGames.Dbg.Draw;
using UnityEngine;

namespace MisterGames.Interact.Detectables {

    public sealed class Detectable : MonoBehaviour, IDetectable {

        [SerializeField] private DetectionStrategy _strategy;

        public event Action<IDetector> OnDetectedBy = delegate {  };
        public event Action<IDetector> OnLostBy = delegate {  };

        public IReadOnlyCollection<IDetector> Observers => _observers;
        public Transform Transform { get; private set; }

        private readonly HashSet<IDetector> _observers = new HashSet<IDetector>();

        private void Awake() {
            Transform = transform;
        }

        private void OnDisable() {
            ForceRemoveAllObservers();
        }

        public bool IsDetectedBy(IDetector detector) {
            return _observers.Contains(detector);
        }

        public bool IsAllowedToStartDetectBy(IDetector detector) {
            return enabled && _strategy.IsAllowedToStartDetection(detector, this);
        }

        public bool IsAllowedToContinueDetectBy(IDetector detector) {
            return enabled && _strategy.IsAllowedToContinueDetection(detector, this);
        }

        public void NotifyDetectedBy(IDetector detector) {
            if (_observers.Contains(detector)) return;

            _observers.Add(detector);
            OnDetectedBy.Invoke(detector);
        }

        public void NotifyLostBy(IDetector detector) {
            if (!_observers.Contains(detector)) return;

            _observers.Remove(detector);
            OnLostBy.Invoke(detector);
        }

        public void ForceRemoveAllObservers() {
            var observers = new IDetector[_observers.Count];
            _observers.CopyTo(observers);
            _observers.Clear();

            for (int i = 0; i < observers.Length; i++) {
                OnLostBy.Invoke(observers[i]);
            }
        }

        public override string ToString() {
            return $"{nameof(Detectable)}({name}, observers count = {_observers.Count})";
        }

        [Header("Debug")]
        [SerializeField] private bool _debugDrawDetectable;

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (!Application.isPlaying || !_debugDrawDetectable) return;

            DbgSphere.Create().Color(Color.yellow).Position(transform.position).Radius(0.4f).Draw();
        }
#endif
    }

}
