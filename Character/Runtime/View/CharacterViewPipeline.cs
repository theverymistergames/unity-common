using MisterGames.Actors;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using MisterGames.UI.Initialization;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CharacterViewPipeline : MonoBehaviour, IActorComponent, IUpdate {

        [SerializeField] private Camera _camera;
        [SerializeField] private CameraContainer _cameraContainer;
        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        [Header("View Settings")]
        [SerializeField] private Vector2 _sensitivity = new Vector2(0.15f, 0.15f);
        [SerializeField] private float _smoothing = 20f;
        [SerializeField] [Min(0f)] private float _freeHeadRotationDistance;
        [SerializeField] private float _returnFreeHeadRotationSmoothing = 5f;
        [SerializeField] private CharacterViewClampProcessor _viewClamp;
        
        public CameraContainer CameraContainer => _cameraContainer;
        public Vector2 Sensitivity { get => _sensitivity; set => _sensitivity = value; }
        public float Smoothing { get => _smoothing; set => _smoothing = value; }

        private ITimeSource _timeSource;
        private ITransformAdapter _headAdapter;
        private ITransformAdapter _bodyAdapter;
        private CharacterInputPipeline _inputPipeline;

        private Vector2 _inputDeltaAccum;
        private Vector2 _currentOrientation;

        void IActorComponent.OnAwake(IActor actor) {
            _headAdapter = actor.GetComponent<CharacterHeadAdapter>();
            _bodyAdapter = actor.GetComponent<CharacterBodyAdapter>();
            _inputPipeline = actor.GetComponent<CharacterInputPipeline>();
            _timeSource = TimeSources.Get(_playerLoopStage);
        }

        private void OnEnable() {
            CanvasRegistry.Instance.SetCanvasEventCamera(_camera);
            _inputPipeline.OnViewVectorChanged += HandleViewVectorChanged;
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            CanvasRegistry.Instance.SetCanvasEventCamera(null);
            _inputPipeline.OnViewVectorChanged -= HandleViewVectorChanged;
            _timeSource.Unsubscribe(this);
        }
        
        public void LookAt(Transform target) {
            _viewClamp.LookAt(target);
        }

        public void LookAt(Vector3 target) {
            _viewClamp.LookAt(target);
        }

        public void StopLookAt() {
            _viewClamp.StopLookAt();
        }

        public void ApplyHorizontalClamp(ViewAxisClamp clamp) {
            _viewClamp.ApplyHorizontalClamp(_currentOrientation, clamp);
        }

        public void ApplyVerticalClamp(ViewAxisClamp clamp) {
            _viewClamp.ApplyVerticalClamp(_currentOrientation, clamp);
        }

        private void HandleViewVectorChanged(Vector2 input) {
            _inputDeltaAccum += new Vector2(-input.y, input.x);
        }

        void IUpdate.OnUpdate(float dt) {
            var current = GetCurrentOrientation();
            var target = current + ConsumeInputDelta();
            
            ApplyClamp(current, ref target, dt);
            ApplySmoothing(ref current, target, dt);
            PerformRotation(current, dt);
        }

        private Vector2 ConsumeInputDelta() {
            var delta = new Vector2(_inputDeltaAccum.x * _sensitivity.x, _inputDeltaAccum.y * _sensitivity.y);
            _inputDeltaAccum = Vector2.zero;
            return delta;
        }

        private void ApplyClamp(Vector2 current, ref Vector2 target, float dt) {
            _viewClamp.Process(_headAdapter.Position, current, ref target, dt);
        }

        private void ApplySmoothing(ref Vector2 current, Vector2 target, float dt) {
            current = Vector2.Lerp(current, target, dt * _smoothing);
        }

        private void PerformRotation(Vector2 eulerAngles, float dt) {
            // If head offset from body is longer than free head rotation distance,
            // body rotation is not applied to prevent head from rotation around body vertical axis. 
            if (_headAdapter.LocalPosition.sqrMagnitude < _freeHeadRotationDistance * _freeHeadRotationDistance) {
                _bodyAdapter.Rotation = Quaternion.Slerp(
                    _bodyAdapter.Rotation,
                    Quaternion.Euler(0f, eulerAngles.y, 0f), 
                    dt * _returnFreeHeadRotationSmoothing
                );
            }
            
            _headAdapter.Rotation = Quaternion.Euler(eulerAngles);
        }

        private Vector2 GetCurrentOrientation() {
            return _headAdapter.Rotation.eulerAngles.ToEulerAngles180();
        }
    }

}
