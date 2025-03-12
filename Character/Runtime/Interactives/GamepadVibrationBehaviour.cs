using System;
using MisterGames.Common.Easing;
using MisterGames.Common.Inputs;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Interactives {
    
    public sealed class GamepadVibrationBehaviour : MonoBehaviour, IUpdate {

        [SerializeField] private LabelValue _priority;
        
        [Header("Weight")]
        [SerializeField] [Range(0f, 1f)] private float _targetWeightLeft;
        [SerializeField] [Range(0f, 1f)] private float _targetWeightRight;
        [SerializeField] [Min(0f)] private float _leftWeightMul = 1f;
        [SerializeField] [Min(0f)] private float _rightWeightMul = 1f;
        [SerializeField] [Min(0f)] private float _weightSmoothingLeft = 3f;
        [SerializeField] [Min(0f)] private float _weightSmoothingRight = 3f;

        [Header("Frequency")]
        [SerializeField] [Range(0f, 1f)] private float _leftFreqStart;
        [SerializeField] [Range(0f, 1f)] private float _leftFreqEnd;
        [SerializeField] [Range(0f, 1f)] private float _rightFreqStart;
        [SerializeField] [Range(0f, 1f)] private float _rightFreqEnd;
        [SerializeField] private OscillatedCurve _leftFreqCurve;
        [SerializeField] private OscillatedCurve _rightFreqCurve;
        
        private float _weightSmoothedLeft;
        private float _weightSmoothedRight;
        private bool _isRegistered;

        public void SetWeight(GamepadSide side, float weight) {
            switch (side) {
                case GamepadSide.Left:
                    _targetWeightLeft = Mathf.Clamp01(weight);
                    break;
                
                case GamepadSide.Right:
                    _targetWeightRight = Mathf.Clamp01(weight);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
            
            Register();
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        public void SetSmoothing(GamepadSide side, float smoothing) {
            switch (side) {
                case GamepadSide.Left:
                    _weightSmoothingLeft = smoothing;
                    break;
                
                case GamepadSide.Right:
                    _weightSmoothingRight = smoothing;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
        
        private void OnEnable() {
            Register();
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDestroy() {
            Unregister();
        }

        private void Register() {
            if (_isRegistered) return;
            
            DeviceService.Instance.GamepadVibration.Register(this, _priority.GetValue());
            _isRegistered = true;
        }

        private void Unregister() {
            if (!_isRegistered) return;
            
            DeviceService.Instance?.GamepadVibration.Unregister(this);
            _isRegistered = false;
        }

        void IUpdate.OnUpdate(float dt) {
            float targetWeightLeft = enabled ? _targetWeightLeft : 0f;
            float targetWeightRight = enabled ? _targetWeightRight : 0f;
            
            _weightSmoothedLeft = _weightSmoothedLeft.SmoothExpNonZero(targetWeightLeft, _weightSmoothingLeft, dt);
            _weightSmoothedRight = _weightSmoothedRight.SmoothExpNonZero(targetWeightRight, _weightSmoothingRight, dt);

            var freq = new Vector2(
                Mathf.Lerp(_leftFreqStart, _leftFreqEnd, _leftFreqCurve.Evaluate(_weightSmoothedLeft)),
                Mathf.Lerp(_rightFreqStart, _rightFreqEnd, _rightFreqCurve.Evaluate(_weightSmoothedRight))
            );
            
            DeviceService.Instance.GamepadVibration.SetTwoMotors(
                this,
                freq,
                _weightSmoothedLeft * _leftWeightMul,
                _weightSmoothedRight * _rightWeightMul
            );
            
            if (_weightSmoothedLeft > 0f || _weightSmoothedRight > 0f) return;

            Unregister();
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (!Application.isPlaying) return;
            
            SetWeight(GamepadSide.Left, _targetWeightLeft);
            SetWeight(GamepadSide.Right, _targetWeightRight);
        }
#endif
    }
    
}