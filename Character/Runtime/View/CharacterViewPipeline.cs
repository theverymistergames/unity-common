using MisterGames.Character.Access;
using MisterGames.Character.Motion;
using MisterGames.Character.Processors;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CharacterViewPipeline : MonoBehaviour, ICharacterViewPipeline, IUpdate {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2[] _inputProcessors = {
            new CharacterProcessorVector2Sensitivity { sensitivity = new Vector2(0.15f, 0.15f) },
        };

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2 _inputToViewProcessor =
            new CharacterProcessorVector2Clamp {
                xMode = ClampMode.Full,
                lowerBounds = new Vector2(-90f, 0f),
                upperBounds = new Vector2(90f, 0f),
            };

        [SerializeReference] [SubclassSelector] private ICharacterProcessorQuaternion[] _viewProcessors = {
            new CharacterProcessorQuaternionSmoothing { smoothFactor = 20f },
        };

        private ITimeSource _timeSource;
        private ITransformAdapter _headAdapter;
        private ITransformAdapter _bodyAdapter;

        private Vector2 _lastViewVector;
        private Vector2 _input;
        private Quaternion _currentView;

        public void SetEnabled(bool isEnabled) {
            if (isEnabled) {
                _characterAccess.Input.OnViewVectorChanged -= HandleViewVectorChanged;
                _characterAccess.Input.OnViewVectorChanged += HandleViewVectorChanged;
                _timeSource.Subscribe(this);
                return;
            }

            _timeSource.Unsubscribe(this);
            _characterAccess.Input.OnViewVectorChanged -= HandleViewVectorChanged;
        }

        public T GetProcessor<T>() where T : class {
            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is T t) return t;
            }
            return null;
        }

        private void Awake() {
            _headAdapter = _characterAccess.HeadAdapter;
            _bodyAdapter = _characterAccess.BodyAdapter;

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

        private void HandleViewVectorChanged(Vector2 input) {
            _lastViewVector += new Vector2(-input.y, input.x);
        }

        public void OnUpdate(float dt) {
            var viewVector = _lastViewVector;
            _lastViewVector = Vector2.zero;

            for (int i = 0; i < _inputProcessors.Length; i++) {
                viewVector = _inputProcessors[i].Process(viewVector, dt);
            }

            _input = _inputToViewProcessor.Process(_input + viewVector, dt);

            var inputQuaternion = Quaternion.Euler(_input.x, _input.y, 0f);
            for (int i = 0; i < _viewProcessors.Length; i++) {
                inputQuaternion = _viewProcessors[i].Process(inputQuaternion, dt);
            }

            var lastView = _currentView;
            _currentView = inputQuaternion;
            var diffEulers = _currentView.eulerAngles - lastView.eulerAngles;

            _headAdapter.Rotate(Quaternion.Euler(diffEulers.x, 0f, 0f));
            _bodyAdapter.Rotate(Quaternion.Euler(0f, diffEulers.y, 0f));
        }
    }

}
