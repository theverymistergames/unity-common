using System;
using MisterGames.Common.Collisions;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class InteractiveUser : MonoBehaviour {

        [SerializeField] private CollisionDetector _physicsRaycaster;
        [SerializeField] private FrameUiRaycaster _uiRaycaster;

        public event Action<Interactive> OnInteractiveDetected = delegate {  };
        public event Action OnInteractiveLost = delegate {  };

        public Interactive PossibleInteractive { get; private set; }

        public Vector3 LastHitPoint => _lastDetectionSource switch {
            DetectionSource.UiRaycaster => _uiRaycaster.CollisionInfo.lastHitPoint,
            _ => _physicsRaycaster.CollisionInfo.lastHitPoint
        };

        public float LastDetectionDistance => _lastDetectionSource switch {
            DetectionSource.UiRaycaster => _uiRaycaster.CollisionInfo.lastDistance,
            _ => _physicsRaycaster.CollisionInfo.lastDistance,
        };

        private DetectionSource _lastDetectionSource;
        private int _lastDetectionFrame;

        private enum DetectionSource {
            PhysicsRaycaster,
            UiRaycaster,
            None
        }

        private void OnEnable() {
            _uiRaycaster.OnTransformChanged += OnUiRaycasterTransformChanged;
            _physicsRaycaster.OnTransformChanged += OnPhysicsRaycasterTransformChanged;
        }

        private void OnDisable() {
            _uiRaycaster.OnTransformChanged -= OnUiRaycasterTransformChanged;
            _physicsRaycaster.OnTransformChanged -= OnPhysicsRaycasterTransformChanged;
        }

        public bool IsDetectedTarget(Interactive interactive) {
            return PossibleInteractive == interactive;
        }

        private void OnPhysicsRaycasterTransformChanged() {
            CheckNewPossibleInteractive(_physicsRaycaster.CollisionInfo, DetectionSource.PhysicsRaycaster);
        }

        private void OnUiRaycasterTransformChanged() {
            CheckNewPossibleInteractive(_uiRaycaster.CollisionInfo, DetectionSource.UiRaycaster);
        }

        private void CheckNewPossibleInteractive(CollisionInfo info, DetectionSource detectionSource) {
            int frame = Time.frameCount;

            if (PossibleInteractive != null) {
                if (frame == _lastDetectionFrame && info.lastDistance > LastDetectionDistance) return;

                PossibleInteractive.OnLostByUser(this);
                OnInteractiveLost.Invoke();
            }

            if (!info.hasContact) {
                PossibleInteractive = null;
                _lastDetectionSource = DetectionSource.None;
                return;
            }

            PossibleInteractive = info.transform.GetComponent<Interactive>();
            if (PossibleInteractive == null) {
                _lastDetectionSource = DetectionSource.None;
                return;
            }

            PossibleInteractive.OnDetectedByUser(this);
            OnInteractiveDetected.Invoke(PossibleInteractive);

            _lastDetectionSource = detectionSource;
            _lastDetectionFrame = frame;
        }
    }

}
