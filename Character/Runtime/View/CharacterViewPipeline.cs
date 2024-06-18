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

        [Header("View Settings")]
        [SerializeField] private Vector2 _sensitivity = new Vector2(0.15f, 0.15f);
        [SerializeField] private float _smoothing = 20f;
        [SerializeField] [Min(0f)] private float _freeHeadRotationDistance;
        [SerializeField] private float _returnFreeHeadRotationSmoothing = 5f;
        [SerializeField] private float _returnFreeHeadRotationSmoothingMax = 20f;
        [SerializeField] private ViewClampProcessor _viewClamp;
        
        public CameraContainer CameraContainer => _cameraContainer;
        public Vector3 CurrentOrientation => _headAdapter.Rotation.eulerAngles.ToEulerAngles180();
        public Vector2 Sensitivity { get => _sensitivity; set => _sensitivity = value; }
        public float Smoothing { get => _smoothing; set => _smoothing = value; }

        private readonly CharacterHeadJoint _headJoint = new();
        
        private ITransformAdapter _headAdapter;
        private ITransformAdapter _bodyAdapter;
        private CharacterInputPipeline _inputPipeline;

        private Vector2 _inputDeltaAccum;

        void IActorComponent.OnAwake(IActor actor) {
            _headAdapter = actor.GetComponent<CharacterHeadAdapter>();
            _bodyAdapter = actor.GetComponent<CharacterBodyAdapter>();
            _inputPipeline = actor.GetComponent<CharacterInputPipeline>();
        }

        private void OnEnable() {
            CanvasRegistry.Instance.SetCanvasEventCamera(_camera);
            _inputPipeline.OnViewVectorChanged += HandleViewVectorChanged;
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            CanvasRegistry.Instance.SetCanvasEventCamera(null);
            _inputPipeline.OnViewVectorChanged -= HandleViewVectorChanged;
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        private void OnDestroy() {
            Detach();
            StopLookAt();
        }
        
        public void Attach(Transform target, Vector3 point, AttachMode mode = AttachMode.OffsetOnly, float smoothing = 0f) {
            _headJoint.Attach(target, point, mode, smoothing);
        }
        
        public void Attach(Vector3 point, float smoothing = 0f) {
            _headJoint.Attach(point, smoothing);
        }

        public void Detach() {
            _headJoint.Detach();
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
            _viewClamp.ApplyHorizontalClamp(CurrentOrientation, clamp);
        }

        public void ApplyVerticalClamp(ViewAxisClamp clamp) {
            _viewClamp.ApplyVerticalClamp(CurrentOrientation, clamp);
        }

        private void HandleViewVectorChanged(Vector2 input) {
            _inputDeltaAccum += new Vector2(-input.y, input.x);
        }

        void IUpdate.OnUpdate(float dt) {
            var currentOrientation = (Vector2) CurrentOrientation;
            var targetOrientation = currentOrientation + ConsumeInputDelta();
            
            ApplyClamp(currentOrientation, ref targetOrientation, dt);
            ApplySmoothing(ref currentOrientation, targetOrientation, dt);
            
            PerformRotation(currentOrientation, dt);
            ApplyHeadJoint(currentOrientation, dt);
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

        private void ApplyHeadJoint(Vector2 current, float dt) {
            var position = _headAdapter.Position;

            _headJoint.Update(ref position, current, dt);
            _headAdapter.Position = position;
        }

        private void PerformRotation(Vector2 eulerAngles, float dt) {
            // If head offset from body is longer than free head rotation distance,
            // body rotation is not applied to prevent head from rotation around body vertical axis. 
            if (_headAdapter.LocalPosition.sqrMagnitude < _freeHeadRotationDistance * _freeHeadRotationDistance) {
                float distance = _headAdapter.LocalPosition.magnitude;
                float t = _freeHeadRotationDistance > 0f ? distance / _freeHeadRotationDistance : 1f;
                float smooth = Mathf.Lerp(_returnFreeHeadRotationSmoothingMax, _returnFreeHeadRotationSmoothing, t);
                
                _bodyAdapter.Rotation = Quaternion.Slerp(
                    _bodyAdapter.Rotation,
                    Quaternion.Euler(0f, eulerAngles.y, 0f), 
                    dt * smooth
                );
            }
            
            _headAdapter.Rotation = Quaternion.Euler(eulerAngles);
        }
    }

}
