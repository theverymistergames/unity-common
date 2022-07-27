using System;
using MisterGames.Common.Collisions;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class InteractiveUser : MonoBehaviour {

        [SerializeField] private CollisionDetector _collisionDetector;

        public event Action<Interactive> OnInteractiveDetected = delegate {  };
        public event Action OnInteractiveLost = delegate {  };

        public Vector3 Handle => _collisionDetector.CollisionInfo.lastHitPoint;

        private Interactive _possibleInteractive;

        private void OnEnable() {
            _collisionDetector.OnTransformChanged += OnTargetTransformChanged;
        }

        private void OnDisable() {
            _collisionDetector.OnTransformChanged -= OnTargetTransformChanged;
        }

        public bool IsDetectedTarget(Interactive interactive) {
            return _possibleInteractive == interactive;
        }

        private void OnTargetTransformChanged() {
            if (_possibleInteractive != null) {
                _possibleInteractive.OnLostByUser(this);
                OnInteractiveLost.Invoke();
            }

            var info = _collisionDetector.CollisionInfo;
            if (!info.hasContact) {
                _possibleInteractive = null;
                return;
            }

            _possibleInteractive = info.transform.GetComponent<Interactive>();
            if (_possibleInteractive == null) return;

            _possibleInteractive.OnDetectedByUser(this);
            OnInteractiveDetected.Invoke(_possibleInteractive);
        }
    }

}
