using MisterGames.Character.Core;
using MisterGames.Character.Input;
using MisterGames.Character.Processors;
using MisterGames.Common.Attributes;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using MisterGames.UI.Initialization;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CharacterViewPipeline : CharacterPipelineBase, ICharacterViewPipeline, IUpdate {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private Camera _camera;
        [SerializeField] private CameraContainer _cameraContainer;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2[] _inputProcessors = {
            new CharacterProcessorVector2Sensitivity { sensitivity = new Vector2(0.15f, 0.15f) },
        };

        [SerializeReference] [SubclassSelector] private ICharacterProcessorVector2 _inputToViewProcessor =
            new CharacterProcessorViewClamp {
                vertical = new ViewAxisClamp {
                    mode = ClampMode.Full,
                    bounds = new Vector2(-90f, 90f),
                }
            };

        [SerializeReference] [SubclassSelector] private ICharacterProcessorQuaternion[] _viewProcessors = {
            new CharacterProcessorQuaternionSmoothing { smoothFactor = 20f },
        };

        public override bool IsEnabled { get => enabled; set => enabled = value; }
        public CameraContainer CameraContainer => _cameraContainer;

        private ITimeSource _timeSource;
        private ITransformAdapter _headAdapter;
        private ITransformAdapter _bodyAdapter;

        private Vector2 _lastViewVector;
        private Vector2 _input;
        private Quaternion _currentView;

        private void Awake() {
            _headAdapter = _characterAccess.HeadAdapter;
            _bodyAdapter = _characterAccess.BodyAdapter;

            _timeSource = TimeSources.Get(_playerLoopStage);

            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is ICharacterAccessInitializable ip) ip.Initialize(_characterAccess);
            }
        }

        private void OnDestroy() {
            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is ICharacterAccessInitializable ip) ip.DeInitialize();
            }
        }

        private void OnEnable() {
            CanvasRegistry.Instance.SetCanvasEventCamera(_camera);

            var input = _characterAccess.GetPipeline<ICharacterInputPipeline>();

            input.OnViewVectorChanged -= HandleViewVectorChanged;
            input.OnViewVectorChanged += HandleViewVectorChanged;

            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            CanvasRegistry.Instance.SetCanvasEventCamera(null);

            _timeSource.Unsubscribe(this);

            var input = _characterAccess.GetPipeline<ICharacterInputPipeline>();
            input.OnViewVectorChanged -= HandleViewVectorChanged;
        }

        public T GetProcessor<T>() where T : ICharacterProcessor {
            for (int i = 0; i < _inputProcessors.Length; i++) {
                if (_inputProcessors[i] is T ip) return ip;
            }

            if (_inputToViewProcessor is T ivp) return ivp;

            for (int i = 0; i < _viewProcessors.Length; i++) {
                if (_viewProcessors[i] is T vp) return vp;
            }

            return default;
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
