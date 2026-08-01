using System;
using MisterGames.Common;
using MisterGames.Common.Data;
using MisterGames.Common.Inputs;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public sealed class SliderHelper : MonoBehaviour, IUpdate, IPointerDownHandler, IPointerUpHandler {
        
        [Header("Slider")]
        [SerializeField] private Slider _slider;
        [SerializeField] private RectTransform _bounds;
        [SerializeField] private Vector4 _boundsOffset;
        
        [Header("Inputs")]
        [SerializeField] private ScrollInput[] _inputs;
        [SerializeField] [Min(0f)] private float _deltaSensitivity = 30f;
        [SerializeField] [Min(0f)] private float _vectorSensitivity = 10f;
        
        [Serializable]
        private struct ScrollInput {
            public InputActionRef inputAction;
            public Optional<InputDeviceType> deviceType;
            public InputMode mode;
            public Axis axis;
            public float sensitivity;
        }
        
        private enum InputMode {
            Delta,
            Vector,
        }
        
        private enum Axis {
            X,
            Y,
        }
        
        private IUiNavigationService _navigationService;
        private IDeviceService _deviceService;

        private bool _isInTopOpenedLayer;
        private bool _isPointerPressed;

        private void Awake() {
            _navigationService = Services.Get<IUiNavigationService>();
            _deviceService = Services.Get<IDeviceService>();
        }

        private void OnEnable() {
            PlayerLoopStage.LateUpdate.Subscribe(this);

            _isInTopOpenedLayer = false;
            _isPointerPressed = false;
            
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;

                OnWindowsHierarchyChanged();
            }
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
            
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged -= OnWindowsHierarchyChanged;
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            _isPointerPressed = true;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            _isPointerPressed = false;
        }

        private void OnWindowsHierarchyChanged() {
            _isInTopOpenedLayer = Services.TryGet(out IUiWindowService service) &&
                                  service.FindClosestParentWindow(_slider.gameObject, includeSelf: true) is { } window && 
                                  service.IsInTopOpenedLayer(window);
        }

        void IUpdate.OnUpdate(float dt) {
            ProcessScroll();
        }

        private void ProcessScroll() {
            if (_isPointerPressed) return;
            
            float inputDelta = !_navigationService.IsUiBlocked && IsFocused() 
                ? GetInputDelta(_inputs) 
                : 0;
            
            _slider.normalizedValue = Mathf.Clamp01(_slider.normalizedValue + inputDelta);
        }

        private bool IsFocused() {
            bool hasCursor = Cursor.visible && Cursor.lockState != CursorLockMode.Locked;
            return _isInTopOpenedLayer && hasCursor && IsFocusedWithCursor() || !hasCursor && IsSliderSelected();
        }

        private bool IsFocusedWithCursor() {
            return UiNavigationUtils.IsCursorInsideRect(_bounds, _boundsOffset);
        }

        private bool IsSliderSelected() {
            return _navigationService.HasSelectedGameObject && _navigationService.CurrentSelectable == _slider;
        }

        private float GetInputDelta(ScrollInput[] inputArray) {
            float vectorMax = 0f;
            float deltaMax = 0f;

            var device = _deviceService.CurrentDevice;
            
            for (int i = 0; i < inputArray.Length; i++) {
                ref var input = ref inputArray[i];
                if (input.deviceType.HasValue && input.deviceType.Value != device) continue;
                
                float value = GetValue(ref input);
                
                switch (input.mode) {
                    case InputMode.Delta:
                        if (Mathf.Abs(value) > Mathf.Abs(deltaMax)) deltaMax = value;
                        break;
                    
                    case InputMode.Vector:
                        if (Mathf.Abs(value) > Mathf.Abs(deltaMax)) vectorMax = value;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            deltaMax *= _deltaSensitivity;
            vectorMax *= _vectorSensitivity;
            
            return vectorMax.IsNearlyZero() ? deltaMax : vectorMax;
        }
        
        private static float GetValue(ref ScrollInput input) {
            var inputAction = input.inputAction.Get();
            var vector = inputAction.ReadValue<Vector2>();
            
            float value = input.axis switch {
                Axis.X => vector.x,
                Axis.Y => vector.y,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            return input.sensitivity * value;
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _slider = GetComponent<Slider>();
        }

        private void OnDrawGizmosSelected() {
            if (_bounds == null) return;

            var rect = _bounds.rect;

            rect.xMin += _boundsOffset.x;
            rect.yMin += _boundsOffset.y;
            rect.xMax -= _boundsOffset.z;
            rect.yMax -= _boundsOffset.w;

            DrawRectGizmo(rect, Color.cyan);
        }

        private void DrawRectGizmo(Rect rect, Color color) {
            var scale = _bounds.lossyScale;
            var size = new Vector2(rect.width * scale.x, rect.height * scale.y);

            DebugExt.DrawRect(_bounds.TransformPoint(rect.center), _bounds.rotation, size, color, gizmo: true);
        }
#endif
    }
    
}