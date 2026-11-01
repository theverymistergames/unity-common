using System;
using MisterGames.Actors;
using MisterGames.Common.Easing;
using MisterGames.Common.Inputs;
using MisterGames.Common.Inputs.DualSense;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.Serialization;

namespace MisterGames.Character.Transport {

    [RequireComponent(typeof(CarController))]
    public sealed class CarTriggerController : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] [Min(0f)] private float _directionSpeedThreshold = 0.1f;
        [SerializeField] private TriggerSetting _right;
        [SerializeField] private TriggerSetting _left;

        [Serializable]
        private sealed class TriggerSetting {
            [FormerlySerializedAs("gas")] public PedalSetting normal;
            public PedalSetting brake;
        }

        [Serializable]
        private sealed class PedalSetting {
            public AnimationCurve curve = EasingType.Linear.ToAnimationCurve();
            public TriggerEffect effect0;
            public TriggerEffect effect1;
        }

        private IDualSenseAdapter _dualSenseAdapter;
        private CarController _carController;

        void IActorComponent.OnAwake(IActor actor) {
            _carController = GetComponent<CarController>();
        }

        private void Awake() {
            _dualSenseAdapter = Services.Get<IDeviceService>().DualSenseAdapter;
        }

        private void OnEnable() {
            _carController.OnEnter += OnEnter;
            _carController.OnExit += OnExit;

            if (_carController.IsEntered) OnEnter();
        }

        private void OnDisable() {
            _carController.OnEnter -= OnEnter;
            _carController.OnExit -= OnExit;

            OnExit();
        }

        private void OnEnter() {
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnExit() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);

            _dualSenseAdapter.SetTriggerEffect(GamepadSide.Right, default);
            _dualSenseAdapter.SetTriggerEffect(GamepadSide.Left, default);
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_dualSenseAdapter.HasController()) return;

            float right = _carController.GasInput;
            float left = _carController.ReverseInput;
            float brake = Mathf.Clamp01(_carController.BrakeInput);

            var rightNormal = EvaluateEffect(_right.normal, right);
            var rightBrake = EvaluateEffect(_right.brake, right);
            var leftNormal = EvaluateEffect(_left.normal, left);
            var leftBrake = EvaluateEffect(_left.brake, left);

            var rightEffect = rightNormal;
            var leftEffect = leftNormal;

            if (right > 0f && left > 0f) {
                if (IsMovingForward(right, left)) {
                    rightEffect = LerpEffect(rightNormal, rightBrake, left);
                    leftEffect = leftBrake;
                }
                else {
                    leftEffect = LerpEffect(leftNormal, leftBrake, right);
                    rightEffect = rightBrake;
                }
            }

            rightEffect = LerpEffect(rightEffect, rightBrake, brake);
            leftEffect = LerpEffect(leftEffect, leftBrake, brake);

            Debug.Log($"CarTriggerController.OnUpdate: f {Time.frameCount}, L {left}, R {right}, brake {brake}");
            
            _dualSenseAdapter.SetTriggerEffect(GamepadSide.Right, rightEffect);
            _dualSenseAdapter.SetTriggerEffect(GamepadSide.Left, leftEffect);
        }

        private bool IsMovingForward(float right, float left) {
            float speed = _carController.ForwardSpeed;

            if (speed > _directionSpeedThreshold) return true;
            if (speed < -_directionSpeedThreshold) return false;

            return right >= left;
        }

        private static TriggerEffect EvaluateEffect(PedalSetting setting, float progress) {
            return LerpEffect(setting.effect0, setting.effect1, setting.curve.Evaluate(Mathf.Clamp01(progress)));
        }

        private static TriggerEffect LerpEffect(TriggerEffect a, TriggerEffect b, float t) {
            var effect = t < 0.5f ? a : b;

            effect.StartPosition = Mathf.Lerp((float) a.StartPosition, (float) b.StartPosition, t);
            effect.EndPosition = Mathf.Lerp((float) a.EndPosition, (float) b.EndPosition, t);
            effect.BeginForce = Mathf.Lerp((float) a.BeginForce, (float) b.BeginForce, t);
            effect.MiddleForce = Mathf.Lerp((float) a.MiddleForce, (float) b.MiddleForce, t);
            effect.EndForce = Mathf.Lerp((float) a.EndForce, (float) b.EndForce, t);
            effect.Frequency = Mathf.Lerp((float) a.Frequency, (float) b.Frequency, t);

            return effect;
        }
    }

}
