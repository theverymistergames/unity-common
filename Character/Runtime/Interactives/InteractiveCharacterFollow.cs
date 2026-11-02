using System.Collections.Generic;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.Interact.Interactives;
using UnityEngine;

namespace MisterGames.Character.Interactives {

    public sealed class InteractiveCharacterFollow : MonoBehaviour, IUpdate {

        [SerializeField] private Interactive _interactive;
        [SerializeField] private Transform _follow;
        [SerializeField] private Vector3 _followOffset;
        [SerializeField] [Min(0f)] private float _offsetSmoothing;
        [SerializeField] private bool _makeKinematic = true;
        [SerializeField] private Collider[] _ignoreColliders;

        private struct FollowData {
            public IInteractiveUser user;
            public Transform root;
            public Rigidbody rigidbody;
            public Collider[] colliders;
            public bool madeKinematic;
            public Vector3 offset;
        }

        private readonly List<FollowData> _followers = new();
        private Rigidbody _followRigidbody;
        private Quaternion _lastRotation;

        private void Awake() {
            _followRigidbody = _follow.GetComponent<Rigidbody>();
        }

        private void OnEnable() {
            _interactive.OnStartInteract += OnStartInteract;
            _interactive.OnStopInteract += OnStopInteract;
        }

        private void OnDisable() {
            _interactive.OnStartInteract -= OnStartInteract;
            _interactive.OnStopInteract -= OnStopInteract;

            for (int i = 0; i < _followers.Count; i++) {
                RestoreUser(_followers[i]);
            }

            _followers.Clear();
            GetUpdateStage().Unsubscribe(this);
        }

        private void OnStartInteract(IInteractiveUser user) {
            if (IndexOf(user) >= 0) return;

            var root = user.Root.transform;
            var data = new FollowData {
                user = user,
                root = root,
                rigidbody = root.TryGetComponent(out Rigidbody rb) ? rb : null,
                colliders = root.GetComponentsInChildren<Collider>(includeInactive: true),
            };

            if (_makeKinematic && data.rigidbody != null && !data.rigidbody.isKinematic) {
                data.rigidbody.isKinematic = true;
                data.madeKinematic = true;
            }

            IgnoreCollisions(data.colliders, ignore: true);

            GetFollowPose(Time.fixedDeltaTime, out var position, out var rotation);
            var userPosition = _followRigidbody != null && data.rigidbody != null ? data.rigidbody.position : root.position;
            data.offset = Quaternion.Inverse(rotation) * (userPosition - position);

            _followers.Add(data);

            if (_followers.Count > 1) return;

            _lastRotation = rotation;
            GetUpdateStage().Subscribe(this);
        }

        private void OnStopInteract(IInteractiveUser user) {
            int index = IndexOf(user);
            if (index < 0) return;

            RestoreUser(_followers[index]);
            _followers.RemoveAt(index);

            if (_followers.Count == 0) GetUpdateStage().Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            GetFollowPose(dt, out var position, out var rotation);
            var rotationDelta = rotation * Quaternion.Inverse(_lastRotation);

            for (int i = 0; i < _followers.Count; i++) {
                var data = _followers[i];

                data.offset = data.offset.SmoothExpNonZero(_followOffset, _offsetSmoothing, dt);
                _followers[i] = data;

                var newPosition = position + rotation * data.offset;

                if (_followRigidbody != null && data.rigidbody != null) {
                    data.rigidbody.MovePosition(newPosition);
                    data.rigidbody.MoveRotation(rotationDelta * data.rigidbody.rotation);
                    continue;
                }

                var newRotation = rotationDelta * data.root.rotation;

                data.root.SetPositionAndRotation(newPosition, newRotation);

                if (data.rigidbody == null) continue;

                data.rigidbody.position = newPosition;
                data.rigidbody.rotation = newRotation;
            }

            _lastRotation = rotation;
        }

        private void GetFollowPose(float dt, out Vector3 position, out Quaternion rotation) {
            if (_followRigidbody == null) {
                _follow.GetPositionAndRotation(out position, out rotation);
                return;
            }

            var angularVelocity = _followRigidbody.angularVelocity;

            position = _followRigidbody.position + _followRigidbody.linearVelocity * dt;
            rotation = Quaternion.AngleAxis(angularVelocity.magnitude * Mathf.Rad2Deg * dt, angularVelocity.normalized) *
                       _followRigidbody.rotation;
        }

        private PlayerLoopStage GetUpdateStage() {
            return _followRigidbody != null ? PlayerLoopStage.FixedUpdate : PlayerLoopStage.LateUpdate;
        }

        private void RestoreUser(FollowData data) {
            IgnoreCollisions(data.colliders, ignore: false);

            if (data.madeKinematic && data.rigidbody != null) data.rigidbody.isKinematic = false;
        }

        private void IgnoreCollisions(Collider[] userColliders, bool ignore) {
            for (int i = 0; i < _ignoreColliders.Length; i++) {
                var interactiveCollider = _ignoreColliders[i];
                if (interactiveCollider == null) continue;

                for (int j = 0; j < userColliders.Length; j++) {
                    var userCollider = userColliders[j];
                    if (userCollider == null) continue;

                    Physics.IgnoreCollision(interactiveCollider, userCollider, ignore);
                }
            }
        }

        private int IndexOf(IInteractiveUser user) {
            for (int i = 0; i < _followers.Count; i++) {
                if (_followers[i].user == user) return i;
            }

            return -1;
        }

#if UNITY_EDITOR
        private void Reset() {
            _interactive = GetComponent<Interactive>();
            FetchIgnoreColliders();
        }

        [Button]
        private void FetchIgnoreColliders() {
            if (_interactive == null) return;

            var colliders = new List<Collider>();
            _interactive.GetComponentsInChildren(includeInactive: true, colliders);
            colliders.RemoveAll(c => c.isTrigger);

            _ignoreColliders = colliders.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

}
