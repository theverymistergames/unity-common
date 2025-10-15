using System;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UiScrollHelper : MonoBehaviour, IUpdate {
        
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private DragEventsHandler _dragEventsHandler;
        [SerializeField] private ScrollEventsHandler _scrollEventsHandler;
        
        [Header("Inputs")]
        [SerializeField] private InputActionRef _moveInput;
        [SerializeField] private InputActionRef _vectorAcceleration;
        [SerializeField] private ScrollInput[] _inputs;

        [Header("Motion")]
        [SerializeField] [Min(0f)] private float _deltaSensitivity = 1f;
        [SerializeField] [Min(0f)] private float _vectorSensitivity = 1f;
        [SerializeField] [Min(0f)] private float _deltaSmoothing = 0f;
        [SerializeField] [Min(0f)] private float _vectorAccelerationMul = 3f;

        [Header("Autoscroll")]
        [SerializeField] private bool _enableAutoScroll = true;
        [SerializeField] [Min(0f)] private float _autoscrollStartDelay = 1f;
        [SerializeField] [Range(0f, 1f)] private float _autoscrollPositionX = 0f;
        [SerializeField] [Range(0f, 1f)] private float _autoscrollPositionY = 0f;
        [SerializeField] [Min(0f)] private float _autoscrollSmoothing = 5f;
        
        [Header("Stick to side")]
        [SerializeField] private bool _enableStickToSide = true;
        [SerializeField] private StickMode _stickMode = StickMode.Bottom;
        [SerializeField] [Min(0f)] private float _stickStartDelay = 2f;
        [SerializeField] [Min(0f)] private float _stickToSideDuration = 0.5f;
        [SerializeField] [Min(0f)] private float _stickSmoothing = 5f;
        [SerializeField] [Min(0f)] private float _sideWidth = 16f;
        [SerializeField] [Min(0f)] private float _sideHeight = 16f;
        
        [Serializable]
        private struct ScrollInput {
            public InputActionRef inputAction;
            public InputMode mode;
            public Axis axis;
            public Vector2 sensitivity;
        }
        
        private enum InputMode {
            Delta,
            Vector,
        }
        
        private enum Axis {
            XY,
            YX,
        }

        [Flags]
        private enum StickMode {
            None = 0,
            Bottom = 1,
            Top = 2,
            Left = 4,
            Right = 8,
        }
        
        public bool EnableAutoScroll { get => _enableAutoScroll; set => _enableAutoScroll = value; }
        
        // x - right, y - left, z - bottom , w - top
        private Vector4 _lastTimeTouchedSide;
        private Vector4 _lastTimeHasInputDirectedFromSide;
        private Vector2 _smoothDelta;
        private float _lastTimeHasInputs;
        private Vector2 _lastNormalizedPosition;

        private void OnEnable() {
            _lastNormalizedPosition = _scrollRect.normalizedPosition;
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
            
            _dragEventsHandler.OnDrag += OnDrag;
            _scrollEventsHandler.OnScroll += OnScroll;
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
            
            _dragEventsHandler.OnDrag -= OnDrag;
            _scrollEventsHandler.OnScroll -= OnScroll;
        }

        private void OnScroll(PointerEventData eventData) {
            UpdateInputDirUsageTime(eventData.scrollDelta, Time.realtimeSinceStartup);
        }

        private void OnDrag(PointerEventData eventData) {
            var delta = _scrollRect.normalizedPosition - _lastNormalizedPosition;
            _lastNormalizedPosition = _scrollRect.normalizedPosition;
            
            UpdateInputDirUsageTime(delta, Time.realtimeSinceStartup);
        }

        void IUpdate.OnUpdate(float dt) {
            var contentRect = _scrollRect.content.rect;
            
            Vector2 vectorMax = default;
            Vector2 deltaMax = default;
            float vectorMul = 1f + (_vectorAcceleration.Get()?.ReadValue<float>() ?? 0f) * _vectorAccelerationMul;
            
            for (int i = 0; i < _inputs.Length; i++) {
                ref var input = ref _inputs[i];
                var value = GetValue(ref input);

                switch (input.mode) {
                    case InputMode.Delta:
                        value *= _deltaSensitivity;
                        if (value.sqrMagnitude > deltaMax.sqrMagnitude) deltaMax = value;
                        break;
                    
                    case InputMode.Vector:
                        value *= _vectorSensitivity * vectorMul;
                        if (value.sqrMagnitude > vectorMax.sqrMagnitude) vectorMax = value;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            var targetDelta = new Vector2(
                contentRect.width > 0f ? (vectorMax.x + deltaMax.x) / contentRect.width : 0f,
                contentRect.height > 0f ? (vectorMax.y + deltaMax.y) / contentRect.height : 0f
            );

            float time = Time.realtimeSinceStartup;
            if (targetDelta != default) _lastTimeHasInputs = time;

            _smoothDelta = _smoothDelta.SmoothExpNonZero(targetDelta, _deltaSmoothing, dt);
            var currentPos = _scrollRect.normalizedPosition;
            var nextPos = currentPos + _smoothDelta;

            nextPos.x = Mathf.Clamp01(nextPos.x);
            nextPos.y = Mathf.Clamp01(nextPos.y);

            if (_enableAutoScroll && time - _lastTimeHasInputs >= _autoscrollStartDelay) {
                var autoscrollTarget = new Vector2(_autoscrollPositionX, _autoscrollPositionY);
                nextPos = nextPos.SmoothExpNonZero(autoscrollTarget, _autoscrollSmoothing, dt);
                
                nextPos.x = Mathf.Clamp01(nextPos.x);
                nextPos.y = Mathf.Clamp01(nextPos.y);
            }
            
            if (_enableStickToSide) {
                var stickTarget = nextPos;
                
                UpdateInputDirUsageTime(targetDelta, time);
                
                if (nextPos.x * contentRect.width <= _sideWidth) _lastTimeTouchedSide.x = time;
                if ((1f - nextPos.x) * contentRect.width <= _sideWidth) _lastTimeTouchedSide.y = time;
                if (nextPos.y * contentRect.height <= _sideHeight) _lastTimeTouchedSide.z = time;
                if ((1f - nextPos.y) * contentRect.height <= _sideHeight) _lastTimeTouchedSide.w = time;
                
                if ((StickMode.Right & _stickMode) == StickMode.Right &&
                    time - _lastTimeTouchedSide.x <= _stickToSideDuration && 
                    time - _lastTimeHasInputDirectedFromSide.x > _stickStartDelay) 
                {
                    stickTarget.x = 0f;
                }

                if ((StickMode.Left & _stickMode) == StickMode.Left &&
                    time - _lastTimeTouchedSide.y <= _stickToSideDuration && 
                    time - _lastTimeHasInputDirectedFromSide.y > _stickStartDelay)
                {
                    stickTarget.x = 1f;
                }

                if ((StickMode.Bottom & _stickMode) == StickMode.Bottom && 
                    time - _lastTimeTouchedSide.z <= _stickToSideDuration && 
                    time - _lastTimeHasInputDirectedFromSide.z > _stickStartDelay)
                {
                    stickTarget.y = 0f;
                }
                
                if ((StickMode.Top & _stickMode) == StickMode.Top &&
                    time - _lastTimeTouchedSide.w <= _stickToSideDuration && 
                    time - _lastTimeHasInputDirectedFromSide.w > _stickStartDelay)
                {
                    stickTarget.y = 1f;
                }
                
                nextPos = nextPos.SmoothExpNonZero(stickTarget, _stickSmoothing, dt);
                
                nextPos.x = Mathf.Clamp01(nextPos.x);
                nextPos.y = Mathf.Clamp01(nextPos.y);
            }
            
            _scrollRect.normalizedPosition = nextPos;
            _lastNormalizedPosition = nextPos;
        }

        private void UpdateInputDirUsageTime(Vector2 targetDelta, float time) {
            if (targetDelta.x > 0f) _lastTimeHasInputDirectedFromSide.x = time;
            if (targetDelta.x < 0f) _lastTimeHasInputDirectedFromSide.y = time;
            if (targetDelta.y > 0f) _lastTimeHasInputDirectedFromSide.z = time;
            if (targetDelta.y < 0f) _lastTimeHasInputDirectedFromSide.w = time;
        }
        
        private static Vector2 GetValue(ref ScrollInput input) {
            var vector = input.inputAction.Get().ReadValue<Vector2>();
            
            var value = input.axis switch {
                Axis.XY => vector,
                Axis.YX => new Vector2(vector.y, vector.x),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            return input.sensitivity.Multiply(value);
        }

#if UNITY_EDITOR
        private void Reset() {
            _scrollRect = GetComponent<ScrollRect>();
        }
#endif
    }
    
}