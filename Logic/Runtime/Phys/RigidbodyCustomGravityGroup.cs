using System;
using System.Collections.Generic;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Logic.Phys {

    [DefaultExecutionOrder(-10_000)]
    public sealed class RigidbodyCustomGravityGroup : MonoBehaviour, IUpdate {

        [Header("Default State")]
        [SerializeField] private Options _options;
        [SerializeField] private StartMode _startMode;

        [Header("Gravity")]
        [SerializeField] private Transform _center;
        [SerializeField] private RigidbodyCustomGravity.Mode _gravityMode = RigidbodyCustomGravity.Mode.CustomGlobalOrPhysics;
        [VisibleIf(nameof(_gravityMode), 3)]
        [SerializeField] private CustomGravitySource _localGravitySource;
        [SerializeField] private bool _useGravity = true;
        [SerializeField] private float _gravityScale = 1f;

        [Header("Torque")]
        [SerializeField] [Min(0f)] private float _torqueMin = 0f;
        [SerializeField] [Min(0f)] private float _torqueMax = 60f;

        [Header("Activation")]
        [SerializeField] [Min(1)] private int _activationsPerStep = 16;

        [Header("Sleep")]
        [SerializeField] [Min(0f)] private float _sleepDelay = 0.5f;
        [SerializeField] [Min(1)] private int _sleepBatchPeriod = 5;
        [SerializeField] [Min(1)] private int _sleepingCheckPeriod = 5;

        [Flags]
        public enum Options {
            UseDefault = 0,
            None = 1,
            SetKinematic = 2,
            ReturnToInitialPosition = 4,
            ContinuousSpeculativeCCD = 8,
            EnableInterpolation = 16,
        }

        private enum StartMode {
            OnEnable,
            OnGravityChanged,
        }

        private readonly struct RigidbodyData {
            public readonly Vector3 position;
            public readonly Quaternion rotation;
            public readonly Options options;
            public readonly bool strictActivationRequired;

            public RigidbodyData(Vector3 position, Quaternion rotation, Options options, bool strictActivationRequired) {
                this.position = position;
                this.rotation = rotation;
                this.options = options;
                this.strictActivationRequired = strictActivationRequired;
            }
        }

        private sealed class Member {
            public readonly Rigidbody rigidbody;
            public readonly RigidbodyData data;
            public readonly float sleepThreshold;
            public readonly float angularEnergyFactor;
            public float restTime;
            public bool isSleeping;
            public bool isPendingActivation;
            public int sleepVerifySteps;

            public Member(Rigidbody rigidbody, RigidbodyData data) {
                this.rigidbody = rigidbody;
                this.data = data;
                sleepThreshold = rigidbody.sleepThreshold;

                var inertia = rigidbody.inertiaTensor;
                angularEnergyFactor = Mathf.Max(inertia.x, Mathf.Max(inertia.y, inertia.z)) / Mathf.Max(rigidbody.mass, MinMass);
            }
        }

        public RigidbodyCustomGravity.Mode GravityMode { get => _gravityMode; set => _gravityMode = value; }
        public bool UseGravity { get => _useGravity; set => _useGravity = value; }
        public Vector3 Gravity => GravityDirection * GravityMagnitude;
        public Vector3 GravityDirection { get; private set; } = Vector3.down;
        public float GravityMagnitude { get; private set; } = GravityMagnitudeDefault;
        public float GravityScale { get => _gravityScale; set => _gravityScale = value; }

        private const float GravityMagnitudeDefault = 9.81f;
        private const int SleepVerificationSteps = 2;
        private const float MinMass = 0.0001f;

        private readonly List<Member> _members = new();
        private readonly Dictionary<Rigidbody, int> _memberIndexMap = new();
        private readonly Queue<Member> _activationQueue = new();

        private Vector3 _lastGravity = Vector3.down * GravityMagnitudeDefault;
        private bool _isCustomGravityActive;
        private int _sleepingCheckStep;
        private int _sleepBatchStep;

        private void OnDestroy() {
            _members.Clear();
            _memberIndexMap.Clear();
            _activationQueue.Clear();
        }

        private void OnEnable() {
            PlayerLoopStage.FixedUpdate.Subscribe(this);

            if (_startMode != StartMode.OnEnable) return;

            _isCustomGravityActive = true;

            for (int i = 0; i < _members.Count; i++) {
                EnqueueActivation(_members[i]);
            }
        }

        private void OnDisable() {
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
            ResetAllMembers();
        }

        public void Register(Rigidbody rigidbody, Options options, bool strictActivationRequired) {
            if (_memberIndexMap.ContainsKey(rigidbody)) return;

            var member = new Member(
                rigidbody,
                new RigidbodyData(rigidbody.position, rigidbody.rotation, CreateOptions(options, _options), strictActivationRequired)
            );

            _memberIndexMap[rigidbody] = _members.Count;
            _members.Add(member);

            if (_isCustomGravityActive) EnqueueActivation(member);
            else SetupRigidbodyInitialState(member);
        }

        public void Unregister(Rigidbody rigidbody) {
            if (!_memberIndexMap.Remove(rigidbody, out int index)) return;

            SetupRigidbodyInitialState(_members[index]);

            int lastIndex = _members.Count - 1;
            if (index < lastIndex) {
                var lastMember = _members[lastIndex];
                _members[index] = lastMember;
                _memberIndexMap[lastMember.rigidbody] = index;
            }

            _members.RemoveAt(lastIndex);
        }

        public void ForceActivate(Rigidbody rigidbody, bool strict) {
            if (!_memberIndexMap.TryGetValue(rigidbody, out int index)) return;

            SetupRigidbodyActiveState(_members[index], strict);
        }

        public void ResetAllMembers() {
            _isCustomGravityActive = false;
            _lastGravity = Vector3.down * GravityMagnitudeDefault;
            _activationQueue.Clear();

            for (int i = 0; i < _members.Count; i++) {
                SetupRigidbodyInitialState(_members[i]);
            }
        }

        private void EnqueueActivation(Member member) {
            if (member.isPendingActivation) return;

            member.isPendingActivation = true;
            _activationQueue.Enqueue(member);
        }

        private void ProcessActivationQueue() {
            int count = 0;

            while (count < _activationsPerStep && _activationQueue.TryDequeue(out var member)) {
                if (!member.isPendingActivation) continue;

                SetupRigidbodyActiveState(member, strict: true);
                count++;
            }
        }

        private static Options CreateOptions(Options optionsOfRigidbody, Options defaultOptions) {
            return optionsOfRigidbody == Options.UseDefault ? defaultOptions : optionsOfRigidbody;
        }

        private static void SetupRigidbodyActiveState(Member member, bool strict) {
            if (!strict && member.data.strictActivationRequired) return;

            var rigidbody = member.rigidbody;
            var data = member.data;

            member.restTime = 0f;
            member.isSleeping = false;
            member.isPendingActivation = false;
            member.sleepVerifySteps = 0;

            rigidbody.isKinematic = false;

            if ((data.options & Options.ContinuousSpeculativeCCD) == Options.ContinuousSpeculativeCCD) {
                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }

            if ((data.options & Options.EnableInterpolation) == Options.EnableInterpolation) {
                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        private static void SetupRigidbodyInitialState(Member member) {
            member.isPendingActivation = false;

            var rigidbody = member.rigidbody;
            if (rigidbody == null) return;

            var data = member.data;

            member.restTime = 0f;
            member.isSleeping = false;
            member.sleepVerifySteps = 0;

            if ((data.options & Options.SetKinematic) == Options.SetKinematic) {
                rigidbody.isKinematic = true;
            }

            if ((data.options & Options.ReturnToInitialPosition) == Options.ReturnToInitialPosition) {
                rigidbody.Sleep();

                rigidbody.position = data.position;
                rigidbody.rotation = data.rotation;

                rigidbody.WakeUp();
            }
        }

        void IUpdate.OnUpdate(float dt) {
            var lastGravity = _lastGravity;
            var gravity = GetGravity(_center.position, out bool useGravity);
            bool changed = NotifyGravityVector(gravity);

            if (!_isCustomGravityActive) {
                if (!changed) return;

                _isCustomGravityActive = true;

                for (int i = 0; i < _members.Count; i++) {
                    EnqueueActivation(_members[i]);
                }
            }

            ProcessActivationQueue();

            if (changed && Vector3.Dot(gravity, lastGravity) < 0f) {
                for (int i = 0; i < _members.Count; i++) {
                    _members[i].rigidbody.AddTorque(Random.Range(_torqueMin, _torqueMax) * Random.onUnitSphere, ForceMode.Acceleration);
                }
            }

            _sleepingCheckStep = (_sleepingCheckStep + 1) % _sleepingCheckPeriod;
            _sleepBatchStep = (_sleepBatchStep + 1) % _sleepBatchPeriod;
            bool isSleepBatchStep = _sleepBatchStep == 0;

            for (int i = 0; i < _members.Count; i++) {
                var member = _members[i];
                if (member.isPendingActivation) continue;

                member.rigidbody.useGravity = useGravity;

                bool isVerifyingSleep = member.sleepVerifySteps > 0;
                if (isVerifyingSleep) member.sleepVerifySteps--;

                if (!_useGravity ||
                    !changed && IsSleeping(member, i, isVerifyingSleep) ||
                    TryPutToSleep(member, changed, isSleepBatchStep && !isVerifyingSleep, dt))
                {
                    continue;
                }

                member.rigidbody.AddForce(gravity, ForceMode.Acceleration);
            }
        }

        private bool IsSleeping(Member member, int index, bool forceCheck) {
            if (member.isSleeping && (forceCheck || (index + _sleepingCheckStep) % _sleepingCheckPeriod == 0)) {
                member.isSleeping = member.rigidbody.IsSleeping();
            }

            return member.isSleeping;
        }

        private bool TryPutToSleep(Member member, bool gravityChanged, bool canSleep, float dt) {
            bool isResting = !gravityChanged && GetMassNormalizedKineticEnergy(member) <= member.sleepThreshold;

            member.restTime = isResting ? Mathf.Min(member.restTime + dt, _sleepDelay) : 0f;
            member.isSleeping = canSleep && isResting && member.restTime >= _sleepDelay;

            if (member.isSleeping) {
                member.sleepVerifySteps = SleepVerificationSteps;
                member.rigidbody.Sleep();
            }

            return member.isSleeping;
        }

        private static float GetMassNormalizedKineticEnergy(Member member) {
            var rigidbody = member.rigidbody;
            return 0.5f * (rigidbody.linearVelocity.sqrMagnitude + member.angularEnergyFactor * rigidbody.angularVelocity.sqrMagnitude);
        }

        private Vector3 GetGravity(Vector3 position, out bool useGravity) {
            switch (_gravityMode) {
                case RigidbodyCustomGravity.Mode.Physics:
                    useGravity = _useGravity && _gravityScale.IsNearlyEqual(1f);
                    return Physics.gravity * _gravityScale;

                case RigidbodyCustomGravity.Mode.CustomGlobalOrPhysics:
                    if (CustomGravity.Main.TryGetGlobalGravity(position, out var g)) {
                        useGravity = false;
                        return g * _gravityScale;
                    }

                    useGravity = _useGravity && _gravityScale.IsNearlyEqual(1f);
                    return Physics.gravity * _gravityScale;

                case RigidbodyCustomGravity.Mode.CustomGlobal:
                    useGravity = false;
                    return CustomGravity.Main.GetGlobalGravity(position, Physics.gravity) * _gravityScale;

                case RigidbodyCustomGravity.Mode.CustomLocal:
                    useGravity = false;
                    return _localGravitySource.GetGravity(position, out _) * _gravityScale;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool NotifyGravityVector(Vector3 gravity) {
            if (gravity == _lastGravity) return false;

            _lastGravity = gravity;

            // Do not change last direction if gravity is zero
            if (gravity == default) {
                GravityMagnitude = 0f;
            }
            else {
                GravityDirection = gravity.normalized;
                GravityMagnitude = gravity.magnitude;
            }

            return true;
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void OnDrawGizmos() {
            if (!Application.isPlaying || !_showDebugInfo) return;

            for (int i = 0; i < _members.Count; i++) {
                var p = _members[i].rigidbody.position;

                DebugExt.DrawSphere(p, 0.05f, Color.white, gizmo: true);

                if (!_isCustomGravityActive) continue;

                DebugExt.DrawRay(p, GravityDirection, Color.white, gizmo: true);
                DebugExt.DrawLabel(p + GravityDirection, $"G = {GravityMagnitude:0.000}");
            }
        }
#endif
    }

}
