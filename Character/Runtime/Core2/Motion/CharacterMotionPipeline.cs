using MisterGames.Character.Core2.Processors;
using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Core2.Motion {

    public sealed class CharacterMotionPipeline : MonoBehaviour, ICharacterMotionPipeline, IUpdate {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2[] _inputProcessors = {
            new CharacterProcessorBackSideSpeedCorrection { speedCorrectionBack = 0.6f, speedCorrectionSide = 0.8f },
            new CharacterProcessorVector2Multiplier { multiplier = 5f },
            new CharacterProcessorVector2Smoothing { smoothFactor = 20f },
        };

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2ToVector3 _inputToMotionProcessor =
            new CharacterProcessorVector2ToCharacterForward();

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector3[] _motionProcessors = {
            new CharacterProcessorMass { gravityForce = 15f, airInertialFactor = 1f, groundInertialFactor = 20f },
        };

        public Vector2 MotionInput => _input;

        private ITimeSource _timeSource;
        private ITransformAdapter _bodyAdapter;

        private Vector2 _input;

        public void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _characterAccess.Input.OnMotionVectorChanged -= HandleMotionInput;
                _characterAccess.Input.OnMotionVectorChanged += HandleMotionInput;
                _timeSource.Subscribe(this);
                return;
            }

            _timeSource.Unsubscribe(this);
            _characterAccess.Input.OnMotionVectorChanged -= HandleMotionInput;
        }

        public T GetProcessor<T>() where T : class {
            if (_inputToMotionProcessor is T imp) return imp;

            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is T ip) return ip;
            }

            for (int i = 0; i < _motionProcessors.Length; i++) {
                if (_motionProcessors[i] is T mp) return mp;
            }

            return null;
        }

        private void Awake() {
            _bodyAdapter = _characterAccess.BodyAdapter;
            _timeSource = TimeSources.Get(_playerLoopStage);

            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is ICharacterProcessorInitializable ip) ip.Initialize(_characterAccess);
            }

            if (_inputToMotionProcessor is ICharacterProcessorInitializable imp) imp.Initialize(_characterAccess);

            for (int i = 0; i < _motionProcessors.Length; i++) {
                if (_motionProcessors[i] is ICharacterProcessorInitializable mp) mp.Initialize(_characterAccess);
            }
        }

        private void OnDestroy() {
            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is ICharacterProcessorInitializable ip) ip.DeInitialize();
            }

            if (_inputToMotionProcessor is ICharacterProcessorInitializable imp) imp.DeInitialize();

            for (int i = 0; i < _motionProcessors.Length; i++) {
                if (_motionProcessors[i] is ICharacterProcessorInitializable mp) mp.DeInitialize();
            }
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void HandleMotionInput(Vector2 input) {
            _input = input;
        }

        public void OnUpdate(float dt) {
            var input = _input;
            for (int i = 0; i < _inputProcessors.Length; i++) {
                input = _inputProcessors[i].Process(input, dt);
            }

            var motion = _inputToMotionProcessor.Process(input, dt);
            for (int i = 0; i < _motionProcessors.Length; i++) {
                motion = _motionProcessors[i].Process(motion, dt);
            }

            _bodyAdapter.Move(motion * dt);
        }
    }

}
