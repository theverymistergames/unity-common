using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public sealed class CharacterMotionPipeline : MonoBehaviour, ICharacterMotionPipeline, IUpdate {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private PlayerLoopStage _playerLoopStage;

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2[] _inputProcessors = {
            new CharacterBackSideSpeedCorrectionProcessor(),
            new CharacterProcessorSpeed(),
            new CharacterProcessorVector2Smoothing(),
        };

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2ToVector3 _inputToMotionProcessor =
            new CharacterProcessorInputToMotion();

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector3[] _motionProcessors = {
            new CharacterProcessorMass(),
        };

        private ITimeSource _timeSource;
        private ICharacterMotionAdapter _motionAdapter;
        private Vector2 _input;

        public void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _characterAccess.Input.Move -= HandleMotionInput;
                _characterAccess.Input.Move += HandleMotionInput;
                _timeSource.Subscribe(this);
                return;
            }

            _timeSource.Unsubscribe(this);
            _characterAccess.Input.Move -= HandleMotionInput;
        }

        public T GetInputProcessor<T>() where T : class, ICharacterProcessorVector2 {
            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is T t) return t;
            }
            return null;
        }

        public T GetMotionProcessor<T>() where T : class, ICharacterProcessorVector3 {
            for (int i = 0; i < _motionProcessors.Length; i++) {
                if (_motionProcessors[i] is T t) return t;
            }
            return null;
        }

        public T GetInputToMotionConverter<T>() where T : class, ICharacterProcessorVector2ToVector3 {
            return _inputToMotionProcessor as T;
        }

        private void Awake() {
            _motionAdapter = _characterAccess.MotionAdapter;
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

            _motionAdapter.Move(motion * dt);
        }
    }

}
