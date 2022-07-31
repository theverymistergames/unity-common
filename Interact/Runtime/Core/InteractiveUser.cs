using System;
using MisterGames.Common.Collisions.Core;
using MisterGames.Dbg.Draw;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class InteractiveUser : MonoBehaviour {

        [SerializeField] private CollisionFilter _collisionFilter = new() { maxDistance = 3f };
        [SerializeField] private CollisionDetector _collisionDetector;

        public event Action<Interactive> OnInteractiveDetected = delegate {  };
        public event Action OnInteractiveLost = delegate {  };

        public Interactive PossibleInteractive { get; private set; }
        public CollisionInfo LastCollisionInfo => _lastCollisionInfo;

        private CollisionInfo _lastCollisionInfo;

        private void OnEnable() {
            _collisionDetector.OnTransformChanged += OnCollisionDetectorTransformChanged;
        }

        private void OnDisable() {
            _collisionDetector.OnTransformChanged -= OnCollisionDetectorTransformChanged;
        }

        public bool IsDetectedTarget(Interactive interactive) {
            return PossibleInteractive == interactive;
        }

        private void OnCollisionDetectorTransformChanged() {
            _collisionDetector.FilterLastResults(_collisionFilter, out _lastCollisionInfo);
            CheckNewPossibleInteractive(_lastCollisionInfo);
        }

        private void CheckNewPossibleInteractive(CollisionInfo info) {
            if (PossibleInteractive != null) {
                PossibleInteractive.OnLostByUser(this);
                OnInteractiveLost.Invoke();
            }

            if (!info.hasContact) {
                PossibleInteractive = null;
                return;
            }

            PossibleInteractive = info.transform.GetComponent<Interactive>();
            if (PossibleInteractive == null) return;

            PossibleInteractive.OnDetectedByUser(this);
            OnInteractiveDetected.Invoke(PossibleInteractive);
        }

        public override string ToString() {
            return $"{nameof(InteractiveUser)}(" +
                   $"{name}" +
                   $", possibleInteractive = {(PossibleInteractive == null ? "null" : $"{PossibleInteractive.name}")}" +
                   $")";
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDrawRaycastHit;

        private void Update() {
            if (_debugDrawRaycastHit) {
                DbgPointer.Create().Color(Color.green).Position(_lastCollisionInfo.lastHitPoint).Size(0.5f).Draw();
            }
        }
#endif
    }

}
