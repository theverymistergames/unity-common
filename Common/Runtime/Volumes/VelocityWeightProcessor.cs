using System;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Common.Volumes {
    
    [Serializable]
    public sealed class VelocityWeightProcessor : IPositionWeightProcessor, IUpdate {

        [SerializeField] private Transform _target;
        [SerializeField] [Min(1)] private int _velocityBufferSize = 4;
        [SerializeField] [Min(0f)] private float _linearVelocityMin;
        [SerializeField] [Min(0f)] private float _linearVelocityMax;
        [SerializeField] [Min(0f)] private float _angularVelocityMin;
        [SerializeField] [Min(0f)] private float _angularVelocityMax;
        [SerializeField] private AnimationCurve _linearVelocityCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeField] private AnimationCurve _angularVelocityCurve = EasingType.Linear.ToAnimationCurve();
        
        private readonly VelocityBuffer _linearVelocityBuffer = new();
        private readonly VelocityBuffer _angularVelocityBuffer = new();

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;

        public void Initialize() {
            _lastPosition = _target.position;
            _lastRotation = _target.rotation;

            _linearVelocityBuffer.ClearBuffer();
            _angularVelocityBuffer.ClearBuffer();

            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        public void DeInitialize() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            var position = _target.position;
            var rotation = _target.rotation;

            _linearVelocityBuffer.WriteIntoBuffer(position - _lastPosition, dt);

            var deltaRotation = rotation * Quaternion.Inverse(_lastRotation);
            deltaRotation.ToAngleAxis(out float angle, out var axis);
            if (angle > 180f) angle -= 360f;

            var angularDiff = Mathf.Approximately(angle, 0f) ? Vector3.zero : axis * angle;
            _angularVelocityBuffer.WriteIntoBuffer(angularDiff, dt);

            _lastPosition = position;
            _lastRotation = rotation;
        }

        public float GetWeight() {
            float linearSpeed = _linearVelocityBuffer.GetVelocity().magnitude;
            float angularSpeed = _angularVelocityBuffer.GetVelocity().magnitude;

            float linearWeight = EvaluateWeight(linearSpeed, _linearVelocityMin, _linearVelocityMax, _linearVelocityCurve);
            float angularWeight = EvaluateWeight(angularSpeed, _angularVelocityMin, _angularVelocityMax, _angularVelocityCurve);

            return Mathf.Max(linearWeight, angularWeight);
        }

        private static float EvaluateWeight(float speed, float min, float max, AnimationCurve curve) {
            float t = Mathf.InverseLerp(min, max, speed);
            return Mathf.Clamp01(curve.Evaluate(t));
        }

        public void OnValidate() {
            _linearVelocityBuffer.SetBufferSize(_velocityBufferSize);
            _angularVelocityBuffer.SetBufferSize(_velocityBufferSize);
        }
    }
    
}