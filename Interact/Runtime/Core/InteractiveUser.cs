using System;
using MisterGames.Common.Collisions;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class InteractiveUser : MonoBehaviour {

        [SerializeField] private CollisionDetector _physicsRaycaster;
        [SerializeField] private FrameUiRaycaster _uiRaycaster;

        public event Action<Interactive> OnInteractiveDetected = delegate {  };
        public event Action OnInteractiveLost = delegate {  };

        public bool HasPossibleInteractive => _possibleInteractive != null;

        public Vector3 LastHitPoint => _lastDetectionSource switch {
            DetectionSource.UiRaycaster => _uiRaycaster.CollisionInfo.lastHitPoint,
            _ => _physicsRaycaster.CollisionInfo.lastHitPoint
        };

        public float LastDetectionDistance => _lastDetectionSource switch {
            DetectionSource.UiRaycaster => _uiRaycaster.CollisionInfo.lastDistance,
            _ => _physicsRaycaster.CollisionInfo.lastDistance,
        };

        private Interactive _possibleInteractive;
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
            return _possibleInteractive == interactive;
        }

        private void OnPhysicsRaycasterTransformChanged() {
            int frame = Time.frameCount;
            var info = _physicsRaycaster.CollisionInfo;

            if (_possibleInteractive == null ||
                frame == _lastDetectionFrame &&
                info.lastDistance < LastDetectionDistance
            ) {
                CheckNewPossibleInteractive(info, DetectionSource.PhysicsRaycaster);
            }

            _lastDetectionFrame = frame;
        }

        private void OnUiRaycasterTransformChanged() {
            int frame = Time.frameCount;
            var info = _uiRaycaster.CollisionInfo;

            if (_possibleInteractive == null ||
                frame == _lastDetectionFrame &&
                info.lastDistance < LastDetectionDistance
            ) {
                CheckNewPossibleInteractive(info, DetectionSource.UiRaycaster);
            }

            _lastDetectionFrame = frame;
        }

        private void CheckNewPossibleInteractive(CollisionInfo info, DetectionSource detectionSource) {
            if (_possibleInteractive != null) {
                _possibleInteractive.OnLostByUser(this);
                OnInteractiveLost.Invoke();
            }

            if (!info.hasContact) {
                _possibleInteractive = null;
                _lastDetectionSource = DetectionSource.None;
                return;
            }

            _possibleInteractive = info.transform.GetComponent<Interactive>();
            if (_possibleInteractive == null) {
                _lastDetectionSource = DetectionSource.None;
                return;
            }

            _possibleInteractive.OnDetectedByUser(this);
            OnInteractiveDetected.Invoke(_possibleInteractive);
            _lastDetectionSource = detectionSource;
        }
    }

}
