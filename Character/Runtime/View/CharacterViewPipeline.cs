using System;
using MisterGames.Actors;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Common.Maths;
using MisterGames.Tick.Core;
using MisterGames.UI.Initialization;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CharacterViewPipeline : MonoBehaviour, IActorComponent, IUpdate {
        
        [SerializeField] private CharacterHeadAdapter _head;
        [SerializeField] private CharacterBodyAdapter _body;
        
        [Header("View Settings")]
        [SerializeField] private Vector2 _sensitivity = new Vector2(0.15f, 0.15f);
        [SerializeField] private float _smoothing = 20f;
        [SerializeField] private float _defaultFov = 70f;
        [SerializeField] [Min(0f)] private float _freeHeadRotationDistance;
        [SerializeField] private float _returnFreeHeadRotationSmoothing = 5f;
        [SerializeField] private float _returnFreeHeadRotationSmoothingMax = 20f;
        [SerializeField] private ViewClampProcessor _viewClamp;
        
        public event Action<float> OnAttach { add => _headJoint.OnAttach += value; remove => _headJoint.OnAttach -= value; }
        public event Action OnDetach { add => _headJoint.OnDetach += value; remove => _headJoint.OnDetach -= value; }

        public Vector3 HeadPosition => _head.Position;
        public bool IsAttached => _headJoint.IsAttached;

        public Vector3 Position {
            get => _head.Position;
            set => _head.Position = value;
        }

        public Vector3 LocalPosition {
            get => _head.LocalPosition;
            set => _head.LocalPosition = value;
        }
        
        public Quaternion Rotation {
            get => _rotation;
            set {
                _rotation = value;
                _head.Rotation = value;
            }
        }

        public Vector3 EulerAngles {
            get => _rotation.ToEulerAngles180();
            set => Rotation = Quaternion.Euler(value);
        }
        
        private readonly CharacterHeadJoint _headJoint = new();
       
        private CameraContainer _cameraContainer;
        private CharacterInputPipeline _inputPipeline;
        
        private CharacterViewData _viewData;
        private Quaternion _rotation = Quaternion.identity;
        private Vector2 _inputDeltaAccum;
        private bool _isHorizontalClampOverriden;
        private bool _isVerticalClampOverriden;
        private bool _isSmoothingOverriden;
        private bool _isSensitivityOverriden;

        void IActorComponent.OnAwake(IActor actor) {
            _cameraContainer = actor.GetComponent<CameraContainer>();
            _inputPipeline = actor.GetComponent<CharacterInputPipeline>();
        }

        void IActorComponent.OnSetData(IActor actor) {
            _viewData = actor.GetData<CharacterViewData>();
            UpdateOverridableParameters();
        }

        private void OnEnable() {
            CanvasRegistry.Instance.SetCanvasEventCamera(_cameraContainer.Camera);
            _inputPipeline.OnViewVectorChanged += HandleViewVectorChanged;
            PlayerLoopStage.LateUpdate.Subscribe(this);
            
            _rotation = _head.Rotation;
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
        
        public void AttachObject(Transform obj, Vector3 point, float smoothing = 0f) {
            _headJoint.AttachObject(obj, point, _head.Position, EulerAngles, smoothing);
        }
        
        public void DetachObject(Transform obj) {
            _headJoint.DetachObject(obj);
        }
        
        public void RotateObject(Transform obj, Vector3 sensitivity, RotationPlane plane = RotationPlane.XY, float smoothing = 0f) {
            _headJoint.RotateObject(obj, EulerAngles, sensitivity, plane, smoothing);
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
            _viewClamp.LookAt(target, EulerAngles, mode, orientation, smoothing);
        }

        public void LookAt(Vector3 target, float smoothing = 0f) {
            _viewClamp.LookAt(target, EulerAngles, smoothing);
        }
        
        public void StopLookAt() {
            _viewClamp.StopLookAt();
            _viewClamp.SetClampCenter(EulerAngles);
        }

        public void ApplyHorizontalClamp(ViewAxisClamp clamp) {
            _isHorizontalClampOverriden = true;
            _viewClamp.SetClampCenter(EulerAngles);
            _viewClamp.ApplyHorizontalClamp(clamp);
        }

        public void ApplyVerticalClamp(ViewAxisClamp clamp) {
            _isVerticalClampOverriden = true;
            _viewClamp.SetClampCenter(EulerAngles);
            _viewClamp.ApplyVerticalClamp(clamp);
        }
        
        public void ResetHorizontalClamp() {
            _isHorizontalClampOverriden = false;
            _viewClamp.SetClampCenter(EulerAngles);
            _viewClamp.ApplyHorizontalClamp(_viewData?.horizontalClamp ?? default);
        }

        public void ResetVerticalClamp() {
            _isVerticalClampOverriden = false;
            _viewClamp.SetClampCenter(EulerAngles);
            _viewClamp.ApplyVerticalClamp(_viewData?.verticalClamp ?? default);
        }

        public void SetClampCenter(Quaternion orientation) {
            _viewClamp.SetClampCenter(orientation.ToEulerAngles180());
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
                _viewClamp.SetClampCenter(EulerAngles);
                _viewClamp.ApplyHorizontalClamp(_viewData?.horizontalClamp ?? default);
            }
            
            if (!_isVerticalClampOverriden) {
                _viewClamp.SetClampCenter(EulerAngles);
                _viewClamp.ApplyVerticalClamp(_viewData?.verticalClamp ?? default);
            }
            
            if (!_isSensitivityOverriden) _sensitivity = _viewData?.sensitivity ?? default;
            if (!_isSmoothingOverriden) _smoothing = _viewData?.viewSmoothing ?? default;
        }

        private void HandleViewVectorChanged(Vector2 input) {
            _inputDeltaAccum += new Vector2(-input.y, input.x);
        }

        void IUpdate.OnUpdate(float dt) {
            var delta = ConsumeInputDelta();
            var currentOrientation = (Vector2) EulerAngles;
            var targetOrientation = currentOrientation + delta;
            
            // To apply position before orientation smoothed
            ApplyHeadJoint(currentOrientation, delta, dt);
            
            ApplyClamp(currentOrientation, ref targetOrientation, dt);
            ApplySmoothing(ref currentOrientation, targetOrientation, dt);

            ApplyRotation(currentOrientation, dt);
            
            // To fetch smoothed orientation
            ApplyHeadJoint(currentOrientation, delta, dt: 0f);
            
            ApplyCameraState();
        }

        private void ApplyCameraState() {
            _cameraContainer.SetBaseFov(_viewData?.fov ?? _defaultFov);
        }

        private Vector2 ConsumeInputDelta() {
            var delta = new Vector2(_inputDeltaAccum.x * _sensitivity.x, _inputDeltaAccum.y * _sensitivity.y);
            _inputDeltaAccum = Vector2.zero;
            return delta;
        }

        private void ApplyClamp(Vector2 current, ref Vector2 target, float dt) {
            _viewClamp.Process(_head.Position, current, ref target, dt);
        }

        private void ApplySmoothing(ref Vector2 current, Vector2 target, float dt) {
            current = _smoothing > 0f 
                ? Quaternion.Slerp(Quaternion.Euler(current), Quaternion.Euler(target), dt * _smoothing).eulerAngles
                : target;
        }

        private void ApplyHeadJoint(Vector2 current, Vector2 delta, float dt) {
            var position = _head.Position;

            _headJoint.Update(ref position, current, delta, dt);
            _head.Position = position;
        }

        private void ApplyRotation(Vector2 eulerAngles, float dt) {
            _rotation = Quaternion.Euler(eulerAngles);
            
            // If head offset from body is longer than free head rotation distance,
            // body rotation is not applied to prevent head from rotation around body vertical axis. 
            if (_head.LocalPosition.sqrMagnitude < _freeHeadRotationDistance * _freeHeadRotationDistance) {
                float distance = _head.LocalPosition.magnitude;
                float t = _freeHeadRotationDistance > 0f ? distance / _freeHeadRotationDistance : 1f;
                float smooth = Mathf.Lerp(_returnFreeHeadRotationSmoothingMax, _returnFreeHeadRotationSmoothing, t);
                
                //_body.Rotation = Quaternion.Slerp(_body.Rotation, Quaternion.Euler(0f, eulerAngles.y, 0f), dt * smooth);
            }

            _body.Rotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
            _head.Rotation = _rotation;
        }
    }

}
