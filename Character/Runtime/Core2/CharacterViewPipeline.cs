using MisterGames.Common.Attributes;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public sealed class CharacterViewPipeline : MonoBehaviour, ICharacterPipeline, IUpdate {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2[] _inputProcessors = {
            new CharacterProcessorVector2Sensitivity { sensitivity = new Vector2(0.15f, 0.15f) },
        };

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2 _inputToViewProcessor =
            new CharacterProcessorVector2Clamp {
                xMode = ClampMode.Both,
                lowerBounds = new Vector2(-90f, 0f),
                upperBounds = new Vector2(90f, 0f),
            };

        [SerializeReference] [SubclassSelector] private ICharacterProcessorQuaternion[] _viewProcessors = {
            new CharacterProcessorQuaternionSmoothing { smoothFactor = 20f },
        };

        private ITimeSource _timeSource;
        private ITransformAdapter _viewAdapter;

        private Vector2 _input;
        private Quaternion _currentView;

        public void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _characterAccess.Input.View -= HandleViewInput;
                _characterAccess.Input.View += HandleViewInput;
                _timeSource.Subscribe(this);
                return;
            }

            _timeSource.Unsubscribe(this);
            _characterAccess.Input.View -= HandleViewInput;
        }

        public T GetProcessor<T>() where T : class {
            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is T t) return t;
            }
            return null;
        }

        private void Awake() {
            _viewAdapter = _characterAccess.MotionAdapter;
            _timeSource = TimeSources.Get(_playerLoopStage);

            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is ICharacterProcessorInitializable ip) ip.Initialize(_characterAccess);
            }
        }

        private void OnDestroy() {
            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is ICharacterProcessorInitializable ip) ip.DeInitialize();
            }
        }

        private void OnEnable() {
            SetEnabled(true);
        }

        private void OnDisable() {
            SetEnabled(false);
        }

        private void HandleViewInput(Vector2 input) {
            float dt = 0f;

            input = new Vector2(-input.y, input.x);
            for (int i = 0; i < _inputProcessors.Length; i++) {
                input = _inputProcessors[i].Process(input, dt);
            }

            _input = _inputToViewProcessor.Process(_input + input, dt);
        }

        public void OnUpdate(float dt) {
            var inputQuaternion = Quaternion.Euler(_input.x, _input.y, 0f);

            for (int i = 0; i < _viewProcessors.Length; i++) {
                inputQuaternion = _viewProcessors[i].Process(inputQuaternion, dt);
            }

            var lastView = _currentView;
            _currentView = inputQuaternion;

            _viewAdapter.Rotate(_currentView * Quaternion.Inverse(lastView));
        }
    }

}
