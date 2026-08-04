using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Character.Input;
using MisterGames.Character.Motion;
using MisterGames.Common.Async;
using MisterGames.Common.Inputs;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CharacterViewPipeline : MonoBehaviour, IActorComponent {

        [Header("Transforms")]
        [SerializeField] private Transform _head;
        [SerializeField] private Rigidbody _body;
        
        [Header("View Settings")]
        [SerializeField] private Vector2 _sensitivityMouse = new(0.15f, 0.15f);
        [SerializeField] private Vector2 _sensitivityGamepad = new(1f, 1f);
        [SerializeField] [Min(0f)] private float _smoothing = 20f;
        [SerializeField] [Min(0f)] private float _defaultFov = 70f;
        [SerializeField] [Min(0f)] private float _freeHeadRotationDistance;
        [SerializeField] private float _returnFreeHeadRotationSmoothing = 5f;
        [SerializeField] private float _returnFreeHeadRotationSmoothingMax = 20f;
        [SerializeField] private ViewClampProcessor _viewClamp;
        [SerializeField] [Min(0f)] private float _startDelay = 0.3f;
        
        [Header("Gravity Settings")]
        [SerializeField] [Min(0f)] private float _gravityDirSmoothing = 6f;

        [Header("Timing")]
        [SerializeField] private LabelValue _timescalePriority;
        
        public bool IsAttached => _headJoint.IsAttached;

        public Vector3 HeadPosition {
            get => _head.position;
            set => _head.position = value;
        }

        public Vector3 HeadLocalPosition {
            get => _head.localPosition;
            set => _head.localPosition = value;
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
        
        public Vector3 BodyPositionTransform {
            get => _body.transform.position;
            set => _body.transform.position = value;
        }
        
        public Quaternion BodyRotation {
            get => _body.rotation;
            set {
                _bodyRotation = Quaternion.Inverse(_gravityRotation) * value;
                _body.rotation = value;
            }
        }

        public Vector3 BodyUp => _body.transform.up;

        private readonly Dictionary<IViewProcessor, int> _processorIndexMap = new();
        private readonly List<IViewProcessor> _processorList = new();
        
        private readonly CharacterHeadJoint _headJoint = new();
        private CancellationTokenSource _enableCts;
        private ITimescaleSystem _timescaleSystem;
        private IDeviceService _deviceService;
        
        private CameraContainer _cameraContainer;
        private CharacterInputPipeline _inputPipeline;
        private CharacterGravity _characterGravity;
        
        private CharacterViewData _viewData;
        private Quaternion _gravityRotation = Quaternion.identity;
        
        private bool _isHorizontalClampOverriden;
        private bool _isVerticalClampOverriden;
        private bool _isSmoothingOverriden;
        private bool _hasGravity;
        
        private Quaternion _headRotation = Quaternion.identity;
        private Quaternion _bodyRotation = Quaternion.identity;
        private Vector2 _inputResidual;

        private float _startTime;

        void IActorComponent.OnAwake(IActor actor) {
            _cameraContainer = actor.GetComponent<CameraContainer>();
            _inputPipeline = actor.GetComponent<CharacterInputPipeline>();
            _hasGravity = actor.TryGetComponent(out _characterGravity);
            
            _startTime = GetTime();

            _timescaleSystem = Services.Get<ITimescaleSystem>();
            _deviceService = Services.Get<IDeviceService>();
        }

        void IActorComponent.OnSetData(IActor actor) {
            _viewData = actor.GetData<CharacterViewData>();
            UpdateOverridableParameters();
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _headRotation = _head.rotation;
            _inputResidual = default;

            StartBodyUpdate(_enableCts.Token).Forget();
            StartHeadUpdate(_enableCts.Token).Forget();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
        }

        private void OnDestroy() {
            Detach();
            StopLookAt();
        }
        
        public void AddProcessor(IViewProcessor processor) {
            if (!_processorIndexMap.TryAdd(processor, _processorList.Count)) return;

            _processorList.Add(processor);
        }

        public void RemoveProcessor(IViewProcessor processor) {
            if (!_processorIndexMap.Remove(processor, out int index)) return;

            _processorList[index] = null;
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
            _headJoint.AttachTo(target, point, mode, _gravityRotation, _headRotation.ToEulerAngles180(), smoothing);
        }
        
        public void AttachTo(Vector3 point, float smoothing = 0f) {
            _headJoint.AttachTo(point, smoothing);
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
        }

        public void LookAt(Vector3 target, float smoothing = 0f) {
            _viewClamp.LookAt(target, _headRotation.ToEulerAngles180(), smoothing);
            _viewClamp.ResetNextViewCenterOffset();
        }

        public void LookAlong(Quaternion orientation, float smoothing = 0f) {
            _viewClamp.LookAlong(orientation, _headRotation.ToEulerAngles180(), smoothing);
            _viewClamp.ResetNextViewCenterOffset();
        }

        public void StopLookAt() {
            _viewClamp.StopLookAt();
            _viewClamp.SetViewOrientation(_headRotation.ToEulerAngles180());
        }

        public void SetViewOrientation(Quaternion orientation, bool moveView = false) {
            _viewClamp.SetViewOrientation((Quaternion.Inverse(_gravityRotation) * orientation).ToEulerAngles180());
            if (!moveView) _viewClamp.ResetNextViewCenterOffset();
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
            _smoothing = _viewData?.viewSmoothing ?? 0f;
        }

        private void UpdateOverridableParameters() {
            if (!_isHorizontalClampOverriden) {
                _viewClamp.ApplyHorizontalClamp(_viewData?.horizontalClamp ?? _viewClamp.Horizontal, _headRotation.ToEulerAngles180());
            }
            
            if (!_isVerticalClampOverriden) {
                _viewClamp.ApplyVerticalClamp(_viewData?.verticalClamp ?? _viewClamp.Vertical, _headRotation.ToEulerAngles180());
            }
            
            if (!_isSmoothingOverriden) _smoothing = _viewData?.viewSmoothing ?? _smoothing;
        }

        private async UniTask StartBodyUpdate(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                ProcessBodyUpdate();
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            }
        }
        
        private async UniTask StartHeadUpdate(CancellationToken cancellationToken) {
            while (!cancellationToken.IsCancellationRequested) {
                float dt = _timescalePriority.TryGetValue(out int value) 
                    ? Time.unscaledDeltaTime * _timescaleSystem.GetTimeScale(value) 
                    : Time.unscaledDeltaTime;
                ProcessHeadUpdate(dt);
                await UniTask.Yield(PlayerLoopTiming.LastUpdate);
            }
        }

        private static float GetTime() {
            return Time.unscaledTime;
        }
        
        private void ProcessBodyUpdate() {
            _body.rotation = _gravityRotation * _bodyRotation;
        }
        
        private void ProcessHeadUpdate(float dt) {
            var position = _head.position;
            var delta = ConsumeInputDelta(dt);
            var currentOrientation = (Vector2) _headRotation.ToEulerAngles180();
            var targetOrientation = currentOrientation + _inputResidual + delta;

            ProcessGravityAlign(dt);

            ApplyClamp(position, ref currentOrientation, ref targetOrientation, dt);

            var wantedDelta = targetOrientation - currentOrientation;
            var lastOrientation = currentOrientation;

            ApplySmoothing(ref currentOrientation, targetOrientation, dt);

            var consumedDelta = new Vector2(
                Mathf.DeltaAngle(lastOrientation.x, currentOrientation.x),
                Mathf.DeltaAngle(lastOrientation.y, currentOrientation.y)
            );

            _inputResidual = dt > 0f ? wantedDelta - consumedDelta : default;

            // reapply clamp to get valid target orientation
            currentOrientation = currentOrientation.ToEulerAngles180();
            targetOrientation = currentOrientation;
            ApplyClamp(position, ref currentOrientation, ref targetOrientation, dt: 0f);

            // reapply clamp to get valid target orientation
            ApplyAttach(ref position, targetOrientation, dt);
            ApplyClamp(position, ref currentOrientation, ref targetOrientation, dt: 0f);
            
            ApplyAttachedObjects(position, targetOrientation, delta, dt);
            ApplyRotation(targetOrientation, dt);
            ApplyPosition(position);
            ApplyCameraState();
            
            CleanupProcessors();
        }

        private Vector2 ConsumeInputDelta(float dt) {
            if (GetTime() < _startTime + _startDelay) return default;

            var device = _deviceService.CurrentDevice;
            var sens = ApplyProcessorsForSensitivity(device);
            var input = _inputPipeline.GetViewInputVector();
            var vector = new Vector2(-input.y, input.x);

            return device switch {
                InputDeviceType.KeyboardMouse => vector * _sensitivityMouse * sens,
                InputDeviceType.Gamepad => vector * _sensitivityGamepad * sens * dt,
                _ => throw new ArgumentOutOfRangeException()
            };
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
            _head.rotation = _gravityRotation * _headRotation;
            
            // If head offset from body is longer than free head rotation distance,
            // body rotation is not applied to prevent head from rotation around body vertical axis. 
            if (_head.localPosition.sqrMagnitude < _freeHeadRotationDistance * _freeHeadRotationDistance) {
                float distance = _head.localPosition.magnitude;
                float t = _freeHeadRotationDistance > 0f ? distance / _freeHeadRotationDistance : 1f;
                float smooth = Mathf.Lerp(_returnFreeHeadRotationSmoothingMax, _returnFreeHeadRotationSmoothing, t);
                
                _bodyRotation = _bodyRotation.SlerpNonZero(Quaternion.Euler(0f, eulerAngles.y, 0f), smooth, dt);
            }
        }

        private void ProcessGravityAlign(float dt) {
            if (!_hasGravity || _characterGravity.IsGravityAlignBlocked) return;
            
            var target = Quaternion.FromToRotation(Vector3.down, _characterGravity.GravityDirection);
            _gravityRotation = _gravityRotation.SlerpNonZero(target, _gravityDirSmoothing, dt);
        }

        private void ApplyPosition(Vector3 position) {
            _head.position = position;
        }
        
        private Vector2 ApplyProcessorsForSensitivity(InputDeviceType deviceType) {
            int count = _processorList.Count;
            var sens = Vector2.one;
            for (int i = 0; i < count; i++) {
                if (_processorList[i] is { } processor) {
                    sens *= processor.GetViewSensitivity(deviceType);
                }
            }
            return sens;
        }

        private void CleanupProcessors() {
            int count = _processorList.Count;
            int validCount = count;
            
            for (int i = count - 1; i >= 0; i--) {
                if (_processorList[i] is null && _processorList[--validCount] is { } swap) {
                    _processorList[i] = swap;
                    _processorIndexMap[swap] = i;
                }
            }
            
            _processorList.RemoveRange(validCount, count - validCount);
        }
    }

}
