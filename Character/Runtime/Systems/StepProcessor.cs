using System;
using System.Diagnostics;
using MisterGames.Character.Configs;
using MisterGames.Character.Core2.Collisions;
using MisterGames.Character.Motion;
using MisterGames.Common.Maths;
using MisterGames.Dbg.Draw;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Systems {

    public class StepProcessor : MonoBehaviour, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.Update;

        [SerializeField] private CharacterAdapter _adapter;
        [SerializeField] private MotionInputProcessor _motionInputProcessor;
        [SerializeField] private CharacterGroundDetector _groundDetector;
        [SerializeField] private CharacterFootstepsSettings _settings;

        public event Action<float> OnStep = delegate {  };

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);

        private float _stepLength;
        private float _stepDistance;

        private void OnEnable() {
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            _timeSource.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            if (!_groundDetector.CollisionInfo.hasContact) {
                _stepDistance = 0f;
                return;
            }
            
            var inputSpeed = _motionInputProcessor.TargetSpeed;
            var realSpeed = _adapter.Velocity.magnitude;
            _stepLength = CalculateStepLength(inputSpeed);

            if (realSpeed.IsNearlyZero() || _stepLength.IsNearlyZero()) {
                _stepDistance = 0f;
                return;
            }

            if (_stepDistance < _stepLength) {
                _stepDistance += realSpeed * dt;
                return;
            }

            _stepDistance = 0f;
            NotifyStep();
        }

        private void NotifyStep() {
            OnStep.Invoke(_stepLength);
#if UNITY_EDITOR
            DbgDrawStep();
#endif
        }
        
        private float CalculateStepLength(float inputSpeed) {
            var x = inputSpeed / _settings.maxCharacterSpeed;
            return _settings.stepLengthMin + _settings.stepLengthMultiplier * _settings.stepLengthBySpeed.Evaluate(x);
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDrawStep;

        [Conditional("UNITY_EDITOR")]
        private void DbgDrawStep() {
            if (_debugDrawStep) {
                DbgPointer.Create().Color(Color.yellow).Time(2f).Position(_groundDetector.CollisionInfo.lastHitPoint).Size(0.5f).Draw();
            }
        }
#endif
        
    }

}
