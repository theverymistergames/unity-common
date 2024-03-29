﻿using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Layers;
using UnityEngine;

namespace MisterGames.Collisions.Triggers {

    public sealed class DirectionalTrigger : MonoBehaviour {

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private bool _useExplicitDirection;

        [VisibleIf(nameof(_useExplicitDirection))]
        [SerializeField] private Transform _explicitDirection;

        public event Action<GameObject> OnTriggeredForward = delegate {  };
        public event Action<GameObject> OnTriggeredBackward = delegate {  };

        private Transform _transform;

        private bool _isTrackingCollider;
        private int _trackedTransformHash;

        private void Awake() {
            _transform = transform;
        }

        private void OnTriggerEnter(Collider other) {
            if (!enabled) return;
            if (_isTrackingCollider || !_layerMask.Contains(other.gameObject.layer)) return;

            _trackedTransformHash = other.transform.GetHashCode();
            _isTrackingCollider = true;
        }

        private void OnTriggerExit(Collider other) {
            if (!enabled) return;

            var go = other.gameObject;
            if (!_isTrackingCollider || !_layerMask.Contains(go.layer)) return;

            var t = other.transform;
            if (t.GetHashCode() != _trackedTransformHash) return;

            _isTrackingCollider = false;

            var orientation = _useExplicitDirection ? _explicitDirection.forward : _transform.forward;
            float angle = Vector3.Angle(t.position - _transform.position, orientation);

            if (angle <= 90f) OnTriggeredForward.Invoke(go);
            else OnTriggeredBackward.Invoke(go);
        }
    }
    
}
