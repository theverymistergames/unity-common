using System;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.GameObjects;
using MisterGames.Dbg.Draw;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Interact.Core {

    public sealed class InteractiveUser : MonoBehaviour, IInteractiveUser, IUpdate {

        [Header("Settings")]
        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;
        [SerializeField] private TransformAdapterBase _transformAdapter;

        [Header("Collision detection")]
        [SerializeField] private CollisionDetectorBase _collisionDetector;
        [SerializeField] private CollisionFilter _collisionFilter = new CollisionFilter { maxDistance = 3f };

        public event Action<IInteractive> OnInteractiveDetected = delegate {  };
        public event Action<IInteractive> OnInteractiveLost = delegate {  };

        public event Action<IInteractive, Vector3> OnStartInteract = delegate {  };
        public event Action<IInteractive> OnStopInteract = delegate {  };

        public GameObject GameObject => gameObject;
        public ITransformAdapter TransformAdapter => _transformAdapter;
        public IInteractive PossibleInteractive { get; private set; }
        public bool IsInteracting => ReferenceEquals(PossibleInteractive?.User, this);

        private CollisionInfo _collisionInfo;

        private void OnEnable() {
            TimeSources.Get(_timeSourceStage).Subscribe(this);
        }

        private void OnDisable() {
            TimeSources.Get(_timeSourceStage).Unsubscribe(this);
        }

        public void StartInteract() {
            if (PossibleInteractive == null || IsInteracting) return;

            var hitPoint = _collisionInfo.lastHitPoint;
            PossibleInteractive.StartInteractWithUser(this, hitPoint);
            OnStartInteract.Invoke(PossibleInteractive, hitPoint);
        }

        public void StopInteract() {
            if (PossibleInteractive == null || !IsInteracting) return;

            PossibleInteractive.StopInteractWithUser(this);
            OnStopInteract.Invoke(PossibleInteractive);
        }

        public void OnUpdate(float dt) {
            var lastInfo = _collisionInfo;

            _collisionDetector.FetchResults();
            _collisionDetector.FilterLastResults(_collisionFilter, out _collisionInfo);

            if (_collisionInfo.IsTransformChanged(lastInfo)) {
                CheckNewPossibleInteractive(_collisionInfo);
            }
        }

        private void CheckNewPossibleInteractive(CollisionInfo info) {
            if (IsInteracting) return;

            if (PossibleInteractive != null) {
                OnInteractiveLost.Invoke(PossibleInteractive);
                PossibleInteractive.LoseByUser(this);
                PossibleInteractive = null;
            }

            if (!info.hasContact) return;

            PossibleInteractive = info.transform.GetComponent<IInteractive>();
            if (PossibleInteractive == null) return;

            OnInteractiveDetected.Invoke(PossibleInteractive);
            PossibleInteractive.DetectByUser(this);
        }

        public override string ToString() {
            return $"{nameof(InteractiveUser)}({name}, possibleInteractive = {PossibleInteractive})";
        }

        [Header("Debug")]
        [SerializeField] private bool _debugDrawRaycastHit;

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (_debugDrawRaycastHit) {
                DbgPointer.Create().Color(Color.green).Position(_collisionInfo.lastHitPoint).Size(0.5f).Draw();
            }
        }
#endif
    }

}
