﻿using System;
using MisterGames.Common.Collisions.Core;
using MisterGames.Common.Collisions.Utils;
using MisterGames.Common.Routines;
using MisterGames.Dbg.Draw;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class InteractiveUser : MonoBehaviour, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private CollisionFilter _collisionFilter = new() { maxDistance = 3f };
        [SerializeField] private CollisionDetector _collisionDetector;

        public event Action<Interactive> OnInteractiveDetected = delegate {  };
        public event Action OnInteractiveLost = delegate {  };

        public Interactive PossibleInteractive { get; private set; }
        public CollisionInfo CurrentCollisionInfo => _currentCollisionInfo;

        private CollisionInfo _currentCollisionInfo;
        private bool _hasPossibleInteractive;

        private void OnEnable() {
            _timeDomain.SubscribeUpdate(this);
        }

        private void OnDisable() {
            _timeDomain.UnsubscribeUpdate(this);
        }

        public bool IsDetectedTarget(Interactive interactive) {
            return _hasPossibleInteractive && PossibleInteractive == interactive;
        }

        public void OnUpdate(float dt) {
            var lastInfo = _currentCollisionInfo;

            _collisionDetector.FetchResults();
            _collisionDetector.FilterLastResults(_collisionFilter, out _currentCollisionInfo);

            if (_currentCollisionInfo.IsTransformChanged(lastInfo)) {
                CheckNewPossibleInteractive(_currentCollisionInfo);
            }
        }

        private void CheckNewPossibleInteractive(CollisionInfo info) {
            if (_hasPossibleInteractive) {
                PossibleInteractive.OnLostByUser(this);
                OnInteractiveLost.Invoke();
            }

            if (!info.hasContact) {
                PossibleInteractive = null;
                _hasPossibleInteractive = false;
                return;
            }

            PossibleInteractive = info.transform.GetComponent<Interactive>();
            _hasPossibleInteractive = PossibleInteractive != null;
            if (!_hasPossibleInteractive) return;

            PossibleInteractive.OnDetectedByUser(this);
            OnInteractiveDetected.Invoke(PossibleInteractive);
        }

        public override string ToString() {
            return $"{nameof(InteractiveUser)}(" +
                   $"{name}" +
                   $", possibleInteractive = {(PossibleInteractive == null ? "null" : $"{PossibleInteractive.name}")}" +
                   ")";
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDrawRaycastHit;

        private void Update() {
            if (_debugDrawRaycastHit) {
                DbgPointer.Create().Color(Color.green).Position(_currentCollisionInfo.lastHitPoint).Size(0.5f).Draw();
            }
        }
#endif
    }

}
