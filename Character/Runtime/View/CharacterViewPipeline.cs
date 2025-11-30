using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Common.Async;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CharacterViewPipeline : MonoBehaviour, IActorComponent {
        
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _body;
        
        [Header("View Settings")]
        [SerializeField] private Vector2 _sensitivity = new(0.15f, 0.15f);
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
        
        [Header("Timescale")]
        [SerializeField] private LabelValue _timescalePriority;
        
        public event Action<float> OnAttach { add => _headJoint.OnAttach += value; remove => _headJoint.OnAttach -= value; }
        public event Action OnDetach { add => _headJoint.OnDetach += value; remove => _headJoint.OnDetach -= value; }

        public bool IsAttached => _headJoint.IsAttached;

        public Vector3 HeadPosition {
            get => _head.position;
            set {
                _head.position = value;
                SnapHeadPositionToParent();
            }
        }

        public Vector3 HeadLocalPosition {
            get => _head.localPosition;
            set {
                _head.localPosition = value;
                SnapHeadPositionToParent();
            }
        }
        
        public Vector3 HeadSmoothPosition {
            get => _headPosition;
            set {
                _headPosition = value;
                _head.position = value + _head.rotation * _headOffset;
            }
        }

        public Quaternion HeadRotation {
            get => _head.rotation;
            set {
                _headRotation = Quaternion.Inverse(_gravityRotation) * value;
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
        private CancellationTokenSource _enableCts;
        private ITimescaleSystem _timescaleSystem;
        
        private CameraContainer _cameraContainer;
        private CharacterInputPipeline _inputPipeline;
        private CharacterGravity _characterGravity;
        private Transform _headParent;
        
        private CharacterViewData _viewData;
        private Quaternion _gravityRotation = Quaternion.identity;
        private Vector2 _inputDeltaAccum;
        private bool _isHorizontalClampOverriden;
        private bool _isVerticalClampOverriden;
        private bool _isSmoothingOverriden;
        private bool _isSensitivityOverriden;
        private bool _hasGravity;
        
        private Vector3 _headPosition;
        private Vector3 _headOffset;
        private Quaternion _headRotation = Quaternion.identity;
        private Quaternion _bodyRotation = Quaternion.identity;

        private float _startTime;

        void IActorComponent.OnAwake(IActor actor) {
            _cameraContainer = actor.GetComponent<CameraContainer>();
            _inputPipeline = actor.GetComponent<CharacterInputPipeline>();
            _hasGravity = actor.TryGetComponent(out _characterGravity);

            _headParent = _head.parent;
            
            _startTime = Time.time;

            _timescaleSystem = Services.Get<ITimescaleSystem>();
        }

        void IActorComponent.OnSetData(IActor actor) {
            _viewData = actor.GetData<CharacterViewData>();
            UpdateOverridableParameters();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _inputPipeline.OnViewVectorChanged += HandleViewVectorChanged;
            
            _head.GetPositionAndRotation(out _headPosition, out _headRotation);

            StartPreUpdate(PlayerLoopTiming.PreLateUpdate, _enableCts.Token).Forget();
            StartPostUpdate(PlayerLoopTiming.LastPostLateUpdate, _enableCts.Token).Forget();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _inputPipeline.OnViewVectorChanged -= HandleViewVectorChanged;
        }

        private void OnDestroy() {
            Detach();
            StopLookAt();
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
            SnapHeadPositionToParent();
        }
        
        public void AttachTo(Vector3 point, float smoothing = 0f) {
            _headJoint.AttachTo(point, smoothing);
            SnapHeadPositionToParent();
        }

        public void Detach() {
            _headJoint.Detach();
        }

        public void ApplyAttachDistance(float distance) {
            _headJoint.AttachDistance = distance;
        }

        public void LookAt(Transform target, LookAtMode mode = LookAtMode.Free, Vector3 orientation = default, float smoothing = 0f) {
            _viewClamp.LookAt(target, _headRotation.ToEulerAngles180(), mode, offset: default, orientation, smoothing);
            _viewClamp.ResetNextViewCenterOffset();
            SnapHeadPositionToParent();
        }

        public void LookAt(Vector3 target, float smoothing = 0f) {
            _viewClamp.LookAt(target, _headRotation.ToEulerAngles180(), smoothing);
            _viewClamp.ResetNextViewCenterOffset();
            SnapHeadPositionToParent();
        }

        public void LookAlong(Quaternion orientation, float smoothing = 0f) {
            _viewClamp.LookAlong(orientation, _headRotation.ToEulerAngles180(), smoothing);
            _viewClamp.ResetNextViewCenterOffset();
            SnapHeadPositionToParent();
        }

        public void StopLookAt() {
            _viewClamp.StopLookAt();
            _viewClamp.SetViewOrientation(_headRotation.ToEulerAngles180());
        }

        public void SetViewOrientation(Quaternion orientation, bool moveView = false) {
            _viewClamp.SetViewOrientation((Quaternion.Inverse(_gravityRotation) * orientation).ToEulerAngles180());
            if (!moveView) _viewClamp.ResetNextViewCenterOffset();
            SnapHeadPositionToParent();
        }

        public void ApplyHorizontalClamp(ViewAxisClamp clamp) {
            _isHorizontalClampOverriden = true;
            _viewClamp.ApplyHorizontalClamp(clamp, _headRotation.ToEulerAngles180());
        }

        public void ApplyVerticalClamp(ViewAxisClamp clamp) {
            _isVerticalClampOverriden = true;
            _viewClamp.ApplyVerticalClamp(clamp, _headRotation.ToEulerAngles180());
        }

        public void ResetHorizontalClamp() {
            _isHorizontalClampOverriden = false;
            _viewClamp.ApplyHorizontalClamp(_viewData?.horizontalClamp ?? default, _headRotation.ToEulerAngles180());
        }

        public void ResetVerticalClamp() {
            _isVerticalClampOverriden = false;
            _viewClamp.ApplyVerticalClamp(_viewData?.verticalClamp ?? default, _headRotation.ToEulerAngles180());
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

        public void ResetHeadOffset() {
            _headOffset = Vector3.zero;
            _head.position = _headPosition;
        }

        private void UpdateOverridableParameters() {
            if (!_isHorizontalClampOverriden) {
                _viewClamp.ApplyHorizontalClamp(_viewData?.horizontalClamp ?? _viewClamp.Horizontal, _headRotation.ToEulerAngles180());
            }
            
            if (!_isVerticalClampOverriden) {
                _viewClamp.ApplyVerticalClamp(_viewData?.verticalClamp ?? _viewClamp.Vertical, _headRotation.ToEulerAngles180());
            }
            
            if (!_isSensitivityOverriden) _sensitivity = _viewData?.sensitivity ?? _sensitivity;
            if (!_isSmoothingOverriden) _smoothing = _viewData?.viewSmoothing ?? _smoothing;
        }

        private void HandleViewVectorChanged(Vector2 input) {
            if (Time.time < _startTime + _startDelay) return;
            
            _inputDeltaAccum += new Vector2(-input.y, input.x);
        }

        private async UniTask StartPreUpdate(PlayerLoopTiming loop, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                PreUpdate(GetDeltaTime());
                await UniTask.Yield(loop);
            }
        }
        
        private async UniTask StartPostUpdate(PlayerLoopTiming loop, CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                PostUpdate(GetDeltaTime());
                await UniTask.Yield(loop);
            }
        }

        private float GetDeltaTime() {
            return _timescalePriority.TryGetValue(out int value) 
                ? Time.unscaledDeltaTime * _timescaleSystem.GetTimeScale(value) 
                : Time.unscaledDeltaTime;
        }
        
        private void PreUpdate(float dt) {
            ProcessGravityAlign(dt);
            ProcessPositionSnap(dt);
            
            var position = _headPosition + _head.rotation * _headOffset;
            var orientation = (Vector2) _headRotation.ToEulerAngles180();
            
            ApplyAttach(ref position, orientation, dt);
            ApplyPosition(position);
        }
        
        private void PostUpdate(float dt) {
            var delta = ConsumeInputDelta();
            var currentOrientation = (Vector2) _headRotation.ToEulerAngles180();
            var targetOrientation = currentOrientation + delta;
            var position = _headPosition + _head.rotation * _headOffset;

            ApplyClamp(position, ref currentOrientation, ref targetOrientation, dt);
            ApplySmoothing(ref currentOrientation, targetOrientation, dt);
            
            // Copy current into target to reapply clamp after smoothing
            targetOrientation = currentOrientation;
            ApplyClamp(position, ref currentOrientation, ref targetOrientation, dt: 0f);
            
            ApplyAttachedObjects(position, targetOrientation, delta, dt);
            ApplyRotation(targetOrientation, dt);
            ApplyPosition(position);
            ApplyCameraState();
        }

        private Vector2 ConsumeInputDelta() {
            var delta = new Vector2(_inputDeltaAccum.x * _sensitivity.x, _inputDeltaAccum.y * _sensitivity.y);
            _inputDeltaAccum = Vector2.zero;
            return delta;
        }

        private void ApplyAttach(ref Vector3 position, Vector2 orientation, float dt) {
            _headJoint.UpdateSelf(ref position, _gravityRotation, orientation, dt);
        }
        
        private void ApplyAttachedObjects(Vector3 position, Vector2 orientation, Vector2 delta, float dt) {
            _headJoint.UpdateAttachedObjects(position, _gravityRotation * Quaternion.Euler(orientation), delta, dt);
        }
        
        private void ApplyClamp(Vector3 position, ref Vector2 current, ref Vector2 target, float dt) {
            _viewClamp.Process(position, _gravityRotation, ref current, ref target, dt);
        }

        private void ApplySmoothing(ref Vector2 current, Vector2 target, float dt) {
            current = Quaternion.Euler(current)
                .SlerpNonZero(Quaternion.Euler(target), _smoothing, dt).eulerAngles;
        }

        private void ApplyCameraState() {
            _cameraContainer.SetBaseFov(_viewData?.fov ?? _defaultFov);
        }

        private void ApplyRotation(Vector2 eulerAngles, float dt) {
            _headRotation = Quaternion.Euler(eulerAngles);
            
            // If head offset from body is longer than free head rotation distance,
            // body rotation is not applied to prevent head from rotation around body vertical axis. 
            if (_head.localPosition.sqrMagnitude < _freeHeadRotationDistance * _freeHeadRotationDistance) {
                float distance = _head.localPosition.magnitude;
                float t = _freeHeadRotationDistance > 0f ? distance / _freeHeadRotationDistance : 1f;
                float smooth = Mathf.Lerp(_returnFreeHeadRotationSmoothingMax, _returnFreeHeadRotationSmoothing, t);
                
                _bodyRotation = _bodyRotation.SlerpNonZero(Quaternion.Euler(0f, eulerAngles.y, 0f), smooth, dt);
            }

            _body.rotation = _gravityRotation * _bodyRotation;
            _head.rotation = _gravityRotation * _headRotation;
        }

        private void ProcessGravityAlign(float dt) {
            if (!_hasGravity || _characterGravity.IsGravityAlignBlocked) return;
            
            var target = Quaternion.FromToRotation(Vector3.down, _characterGravity.GravityDirection);
            _gravityRotation = _gravityRotation.SlerpNonZero(target, _gravityDirSmoothing, dt);
        }

        private void ProcessPositionSnap(float dt) {
            if (_headJoint.IsAttached) return;
            
            _headPosition = _headPosition.SmoothExpNonZero(_headParent.position, _positionSmoothing, dt);
        }

        private void ApplyPosition(Vector3 position) {
            _head.position = position;
            _headOffset = _head.InverseTransformDirection(position - _headPosition);
        }
        
        private void SnapHeadPositionToParent() {
            _headPosition = _headParent.position;
            _headOffset = _head.InverseTransformDirection(_head.position - _headPosition);
        }
    }

}
