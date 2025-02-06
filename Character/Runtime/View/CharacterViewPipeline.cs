using System;
using MisterGames.Actors;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.UI.Initialization;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CharacterViewPipeline : MonoBehaviour, IActorComponent, IUpdate {
        
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _body;
        
        [Header("View Settings")]
        [SerializeField] private Vector2 _sensitivity = new Vector2(0.15f, 0.15f);
        [SerializeField] [Min(0f)] private float _smoothing = 20f;
        [SerializeField] [Min(0f)] private float _positionSmoothing = 30f;
        [SerializeField] [Min(0f)] private float _defaultFov = 70f;
        [SerializeField] [Min(0f)] private float _freeHeadRotationDistance;
        [SerializeField] private float _returnFreeHeadRotationSmoothing = 5f;
        [SerializeField] private float _returnFreeHeadRotationSmoothingMax = 20f;
        [SerializeField] private ViewClampProcessor _viewClamp;
        [SerializeField] [Min(0f)] private float _startDelay = 0.3f;
        
        [Header("Gravity Settings")]
        [SerializeField] [Min(0f)] private float _gravityDirSmoothing = 6f;
        
        public event Action<float> OnAttach { add => _headJoint.OnAttach += value; remove => _headJoint.OnAttach -= value; }
        public event Action OnDetach { add => _headJoint.OnDetach += value; remove => _headJoint.OnDetach -= value; }

        public bool IsAttached => _headJoint.IsAttached;

        public Vector3 HeadPosition {
            get => _head.position;
            set {
                _head.position = value;
                _headPosition = _headParent.position;
                _headOffset = value - _headPosition;
            }
        }

        public Vector3 HeadLocalPosition {
            get => _head.localPosition;
            set {
                _head.localPosition = value;
                _headPosition = _headParent.position;
                _headOffset = _head.position - _headPosition;
            }
        }

        public Quaternion HeadRotation {
            get => _head.rotation;
            set {
                _headRotation = Quaternion.Inverse(_gravityOffset) * value;
                _head.rotation = value;
            }
        }
        
        public Vector3 BodyPosition {
            get => _body.position;
            set => _body.position = value;
        }
        
        public Quaternion BodyRotation {
            get => _body.rotation;
            set => _body.rotation = value;
        }

        public Vector3 BodyUp => _body.up;
        
        private readonly CharacterHeadJoint _headJoint = new();
       
        private CameraContainer _cameraContainer;
        private CharacterInputPipeline _inputPipeline;
        private CharacterGravity _characterGravity;
        private Transform _headParent;
        
        private CharacterViewData _viewData;
        private Quaternion _gravityOffset = Quaternion.identity;
        private Vector2 _inputDeltaAccum;
        private bool _isHorizontalClampOverriden;
        private bool _isVerticalClampOverriden;
        private bool _isSmoothingOverriden;
        private bool _isSensitivityOverriden;
        
        private Vector3 _headPosition;
        private Vector3 _headOffset;
        private Quaternion _headRotation = Quaternion.identity;

        private float _startTime;

        void IActorComponent.OnAwake(IActor actor) {
            _cameraContainer = actor.GetComponent<CameraContainer>();
            _inputPipeline = actor.GetComponent<CharacterInputPipeline>();
            _characterGravity = actor.GetComponent<CharacterGravity>();

            _headParent = _head.parent;
            
            _startTime = Time.time;
        }

        void IActorComponent.OnSetData(IActor actor) {
            _viewData = actor.GetData<CharacterViewData>();
            UpdateOverridableParameters();
        }

        private void OnEnable() {
            CanvasRegistry.Instance.SetCanvasEventCamera(_cameraContainer.Camera);
            _inputPipeline.OnViewVectorChanged += HandleViewVectorChanged;
            PlayerLoopStage.UnscaledUpdate.Subscribe(this);
            
            _head.GetPositionAndRotation(out _headPosition, out _headRotation);
        }

        private void OnDisable() {
            CanvasRegistry.Instance.SetCanvasEventCamera(null);
            _inputPipeline.OnViewVectorChanged -= HandleViewVectorChanged;
            PlayerLoopStage.UnscaledUpdate.Unsubscribe(this);
        }

        private void OnDestroy() {
            Detach();
            StopLookAt();
        }

        public void PublishHeadPosition() {
            _headPosition = _headParent.position;
        }
        
        public void AttachObject(Transform obj, Vector3 point, float smoothing = 0f) {
            _headJoint.AttachObject(obj, point, _head.position, _headRotation.ToEulerAngles180(), smoothing);
        }
        
        public void DetachObject(Transform obj) {
            _headJoint.DetachObject(obj);
        }
        
        public void RotateObject(Transform obj, Vector3 sensitivity, RotationPlane plane = RotationPlane.XY, float smoothing = 0f) {
            _headJoint.RotateObject(obj, _headRotation.ToEulerAngles180(), sensitivity, plane, smoothing);
        }

        public void StopRotateObject(Transform obj) {
            _headJoint.StopRotateObject(obj);
        }
        
        public void AttachTo(Transform target, Vector3 point, AttachMode mode = AttachMode.OffsetOnly, float smoothing = 0f) {
            _headJoint.AttachTo(target, point, mode, smoothing);
        }
        
        public void AttachTo(Vector3 point, float smoothing = 0f) {
            _headJoint.AttachTo(point, smoothing);
        }

        public void Detach() {
            _headJoint.Detach();
        }
        
        public void LookAt(Transform target, LookAtMode mode = LookAtMode.Free, Vector3 orientation = default, float smoothing = 0f) {
            _viewClamp.LookAt(target, _headRotation.ToEulerAngles180(), mode, orientation, smoothing);
        }

        public void LookAt(Vector3 target, float smoothing = 0f) {
            _viewClamp.LookAt(target, _headRotation.ToEulerAngles180(), smoothing);
        }
        
        public void StopLookAt() {
            _viewClamp.StopLookAt();
            _viewClamp.SetClampCenter(_headRotation.ToEulerAngles180());
        }

        public void ApplyHorizontalClamp(ViewAxisClamp clamp) {
            _isHorizontalClampOverriden = true;
            _viewClamp.SetClampCenter(_headRotation.ToEulerAngles180());
            _viewClamp.ApplyHorizontalClamp(clamp);
        }

        public void ApplyVerticalClamp(ViewAxisClamp clamp) {
            _isVerticalClampOverriden = true;
            _viewClamp.SetClampCenter(_headRotation.ToEulerAngles180());
            _viewClamp.ApplyVerticalClamp(clamp);
        }
        
        public void ResetHorizontalClamp() {
            _isHorizontalClampOverriden = false;
            _viewClamp.SetClampCenter(_headRotation.ToEulerAngles180());
            _viewClamp.ApplyHorizontalClamp(_viewData?.horizontalClamp ?? default);
        }

        public void ResetVerticalClamp() {
            _isVerticalClampOverriden = false;
            _viewClamp.SetClampCenter(_headRotation.ToEulerAngles180());
            _viewClamp.ApplyVerticalClamp(_viewData?.verticalClamp ?? default);
        }

        public void SetClampCenter(Quaternion orientation) {
            _viewClamp.SetClampCenter((Quaternion.Inverse(_gravityOffset) * orientation).ToEulerAngles180());
        }

        public void ApplySmoothing(float smoothing) {
            _isSmoothingOverriden = true;
            _smoothing = smoothing;
        }
        
        public void ResetSmoothing() {
            _isSmoothingOverriden = false;
            _smoothing = _viewData?.viewSmoothing ?? default;
        }

        public void ApplySensitivity(Vector2 sensitivity) {
            _isSensitivityOverriden = true;
            _sensitivity = sensitivity;
        }

        public void ResetSensitivity() {
            _isSensitivityOverriden = false;
            _sensitivity = _viewData?.sensitivity ?? default;
        }
        
        public void ApplyAttachDistance(float distance) {
            _headJoint.AttachDistance = distance;
        }

        private void UpdateOverridableParameters() {
            if (!_isHorizontalClampOverriden) {
                _viewClamp.SetClampCenter(_headRotation.ToEulerAngles180());
                _viewClamp.ApplyHorizontalClamp(_viewData?.horizontalClamp ?? default);
            }
            
            if (!_isVerticalClampOverriden) {
                _viewClamp.SetClampCenter(_headRotation.ToEulerAngles180());
                _viewClamp.ApplyVerticalClamp(_viewData?.verticalClamp ?? default);
            }
            
            if (!_isSensitivityOverriden) _sensitivity = _viewData?.sensitivity ?? default;
            if (!_isSmoothingOverriden) _smoothing = _viewData?.viewSmoothing ?? default;
        }

        private void HandleViewVectorChanged(Vector2 input) {
            if (Time.time < _startTime + _startDelay) return;
            
            _inputDeltaAccum += new Vector2(-input.y, input.x);
        }

        void IUpdate.OnUpdate(float dt) {
            ProcessGravity(dt);
            ProcessPosition(dt);
            
            var delta = ConsumeInputDelta();
            var currentOrientation = (Vector2) _headRotation.ToEulerAngles180();
            var targetOrientation = currentOrientation + delta;
            var position = _headPosition + _headOffset;
            
            // To apply position before orientation smoothed
            ApplyHeadJoint(ref position, currentOrientation, delta, dt);
            ApplyClamp(position, currentOrientation, ref targetOrientation, dt);
            
            ApplySmoothing(ref currentOrientation, targetOrientation, dt);

            // To fetch smoothed orientation
            ApplyHeadJoint(ref position, currentOrientation, delta, dt: 0f);
            

            ApplyRotation(currentOrientation, dt);
            ApplyPosition(position);
            ApplyCameraState();
        }
        
        private void ProcessGravity(float dt) {
            var target = Quaternion.FromToRotation(Vector3.down, _characterGravity.GravityDir);
            _gravityOffset = _gravityOffset.SlerpNonZero(target, _gravityDirSmoothing, dt);
        }

        private void ProcessPosition(float dt) {
            _headPosition = _headPosition.SmoothExpNonZero(_headParent.position, _positionSmoothing, dt);
        }

        private void ApplyCameraState() {
            _cameraContainer.SetBaseFov(_viewData?.fov ?? _defaultFov);
        }

        private Vector2 ConsumeInputDelta() {
            var delta = new Vector2(_inputDeltaAccum.x * _sensitivity.x, _inputDeltaAccum.y * _sensitivity.y);
            _inputDeltaAccum = Vector2.zero;
            return delta;
        }

        private void ApplyClamp(Vector3 position, Vector2 current, ref Vector2 target, float dt) {
            _viewClamp.Process(position, _gravityOffset, current, ref target, dt);
        }

        private void ApplySmoothing(ref Vector2 current, Vector2 target, float dt) {
            current = Quaternion.Euler(current)
                .SlerpNonZero(Quaternion.Euler(target), _smoothing, dt)
                .eulerAngles;
        }

        private void ApplyHeadJoint(ref Vector3 position, Vector2 current, Vector2 delta, float dt) {
            _headJoint.Update(ref position, _gravityOffset, current, delta, dt);
        }

        private void ApplyRotation(Vector2 eulerAngles, float dt) {
            _headRotation = Quaternion.Euler(eulerAngles);
            
            // If head offset from body is longer than free head rotation distance,
            // body rotation is not applied to prevent head from rotation around body vertical axis. 
            if (_head.localPosition.sqrMagnitude < _freeHeadRotationDistance * _freeHeadRotationDistance) {
                float distance = _head.localPosition.magnitude;
                float t = _freeHeadRotationDistance > 0f ? distance / _freeHeadRotationDistance : 1f;
                float smooth = Mathf.Lerp(_returnFreeHeadRotationSmoothingMax, _returnFreeHeadRotationSmoothing, t);
                
                var target = _gravityOffset * Quaternion.Euler(0f, eulerAngles.y, 0f);
                _body.rotation = Quaternion.Slerp(_body.rotation, target, dt * smooth);
            }

            _head.rotation = _gravityOffset * _headRotation;
        }

        private void ApplyPosition(Vector3 position) {
            _head.position = position;
        }
    }

}
