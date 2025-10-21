using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using MisterGames.Input.Actions;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.UI.Components {
    
    public sealed class UiScrollHelper : MonoBehaviour, IUpdate {
        
        [SerializeField] private EnableMode _enableMode = EnableMode.OnFocus;
        
        [Header("Scroll Rect")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private PointerEventsHandler[] _pointerEventHandlers;
        
        [Header("Inputs")]
        [SerializeField] private InputActionRef _accelerateInput;
        [SerializeField] private ScrollInput[] _inputs;

        [Header("Motion")]
        [SerializeField] [Min(0f)] private float _deltaSensitivity = 301f;
        [SerializeField] [Min(0f)] private float _vectorSensitivity = 10f;
        [SerializeField] [Min(0f)] private float _deltaSmoothing = 0f;
        [SerializeField] [Min(0f)] private float _accelerationMul = 3f;

        [Header("Move To Position")]
        [SerializeField] private EasingType _moveToPositionEasing = EasingType.EaseInOutSine;
        
        [Header("Auto Scroll")]
        [SerializeField] private bool _enableAutoScroll = true;
        [SerializeField] [Min(0f)] private float _autoscrollStartDelay = 1f;
        [SerializeField] [Range(0f, 1f)] private float _autoscrollPositionX = 0f;
        [SerializeField] [Range(0f, 1f)] private float _autoscrollPositionY = 0f;
        [SerializeField] [Min(0f)] private float _autoscrollSmoothing = 5f;
        
        [Header("Stick To Side")]
        [SerializeField] private bool _enableStickToSide = true;
        [SerializeField] private StickMode _stickMode = StickMode.Bottom;
        [SerializeField] private StickMode _initialStickMode = StickMode.Bottom;
        [SerializeField] [Min(0f)] private float _stickStartDelay = 1f;
        [SerializeField] [Min(0f)] private float _stickSpeed = 10f;
        [SerializeField] [Min(0f)] private float _sideWidth = 100f;
        [SerializeField] [Min(0f)] private float _sideHeight = 100f;
        
        [Serializable]
        private struct ScrollInput {
            public InputActionRef inputAction;
            public InputMode mode;
            public Axis axis;
            public Vector2 sensitivity;
        }

        private enum EnableMode {
            OnEnable,
            OnFocus,
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
        
        private IUiNavigationService _navigationService;
        
        // x - right, y - left, z - bottom , w - top
        private Vector4 _lastTimeNotTouchedSide;
        private Vector2 _velocity;
        private float _lastTimeHasInputs;
        private Vector2 _stickDir;
        private Vector2 _lastInputDir;
        
        private float _moveToPositionStartTime;
        private float _moveToPositionDuration;
        private Vector2 _startMovePosition;
        private Vector2 _targetMovePositionNormalized;
        
        private IUiNavigationNode _parentNode;
        private bool _isInTopOpenedLayer;
        private bool _containsSelectedObjectDirectly;

        private bool _isPointerPressed;

        private void Awake() {
            _navigationService = Services.Get<IUiNavigationService>();
        }

        private void OnEnable() {
            PlayerLoopStage.LateUpdate.Subscribe(this);

            _isInTopOpenedLayer = false;
            _containsSelectedObjectDirectly = false;
            _parentNode = null;
            _isPointerPressed = false;
            _lastTimeHasInputs = 0f;
            _lastTimeNotTouchedSide = default;
            _lastInputDir = default;
            _stickDir = new Vector2(
                (_initialStickMode & StickMode.Right) != 0 ? -1f 
                : (_initialStickMode & StickMode.Left) != 0 ? 1f 
                : 0f,
                (_initialStickMode & StickMode.Bottom) != 0 ? -1f 
                : (_initialStickMode & StickMode.Top) != 0 ? 1f
                : 0f
            );

            ResetMoveToPosition();
            
            for (int i = 0; i < _pointerEventHandlers.Length; i++) {
                _pointerEventHandlers[i].OnPointerUp += OnPointerUp;
                _pointerEventHandlers[i].OnPointerDown += OnPointerDown;
            }
            
            if (Services.TryGet(out IUiNavigationService navigationService)) {
                navigationService.OnNavigationHierarchyChanged += OnNavigationHierarchyChanged;
                navigationService.OnSelectedGameObjectChanged += OnSelectedGameObjectChanged;

                OnNavigationHierarchyChanged();
                OnSelectedGameObjectChanged(navigationService.SelectedGameObject, navigationService.SelectedGameObjectWindow);
            }
            
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged += OnWindowsHierarchyChanged;

                OnWindowsHierarchyChanged();
            }
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
            
            for (int i = 0; i < _pointerEventHandlers.Length; i++) {
                _pointerEventHandlers[i].OnPointerUp -= OnPointerUp;
                _pointerEventHandlers[i].OnPointerDown -= OnPointerDown;
            }
            
            if (Services.TryGet(out IUiNavigationService service)) {
                service.OnNavigationHierarchyChanged -= OnNavigationHierarchyChanged;
                service.OnSelectedGameObjectChanged -= OnSelectedGameObjectChanged;
            }
            
            if (Services.TryGet(out IUiWindowService windowService)) {
                windowService.OnWindowsHierarchyChanged -= OnWindowsHierarchyChanged;
            }
        }

        public void MoveToPosition(Vector2 normalizedPosition, float duration) {
            _startMovePosition = _scrollRect.content.anchoredPosition;
            
            _targetMovePositionNormalized = normalizedPosition.Clamp01();
            _targetMovePositionNormalized.y = 1f - _targetMovePositionNormalized.y;
            
            _moveToPositionStartTime = Time.realtimeSinceStartup;
            _moveToPositionDuration = duration;
        }

        public void ResetMoveToPosition() {
            _moveToPositionStartTime = 0f;
            _moveToPositionDuration = 0f;
        }

        private void OnPointerDown(PointerEventData eventData) {
            _isPointerPressed = true;
        }
        
        private void OnPointerUp(PointerEventData eventData) {
            _isPointerPressed = false;
        }

        private void OnWindowsHierarchyChanged() {
            _isInTopOpenedLayer = Services.TryGet(out IUiWindowService service) &&
                                  service.FindClosestParentWindow(_scrollRect.gameObject, includeSelf: true) is { } window && 
                                  service.IsInTopOpenedLayer(window);
        }

        private void OnNavigationHierarchyChanged() {
            _parentNode = _navigationService?.FindClosestParentNavigationNode(_scrollRect.gameObject, includeSelf: true);
        }

        private void OnSelectedGameObjectChanged(GameObject gameObject, IUiWindow parentWindow) {
            _containsSelectedObjectDirectly = Services.TryGet(out IUiNavigationService navigationService) &&
                                              ContainsSelectedObjectDirectly(navigationService);
        }

        void IUpdate.OnUpdate(float dt) {
            ProcessScroll(dt);
        }

        private void ProcessScroll(float dt) {
            var contentRect = _scrollRect.content.rect;
            var viewportRect = _scrollRect.viewport.rect;
            
            bool hasScrollSpace = _scrollRect.horizontal && contentRect.width > viewportRect.width || 
                                  _scrollRect.vertical && contentRect.height > viewportRect.height;
            
            float time = Time.realtimeSinceStartup;
            
            if (_isPointerPressed || !hasScrollSpace) {
                _velocity = default;
                _lastTimeHasInputs = time;
                _lastInputDir = default;

                if (hasScrollSpace) {
                    _lastTimeNotTouchedSide = time.ToVectorXYZW();
                    _stickDir = default;
                }
                
                ResetMoveToPosition();
                return;
            }
            
            var inputDelta = GetInputDelta();
            var currentPos = _scrollRect.content.anchoredPosition;

            if (!_isInTopOpenedLayer || 
                _enableMode == EnableMode.OnFocus && inputDelta != default && !IsFocused()) 
            {
                inputDelta = default;
            }

            if (inputDelta != default) {
                _lastTimeHasInputs = time;
                _lastInputDir = new Vector2(Mathf.Sign(inputDelta.x), Mathf.Sign(inputDelta.y));
            }
            
            ProcessStickToSide(ref inputDelta);
            
            _velocity = _velocity.SmoothExpNonZero(inputDelta, _deltaSmoothing, dt);
            var vel = new Vector2(_scrollRect.horizontal.AsFloat() * _velocity.x, _scrollRect.vertical.AsFloat() * -_velocity.y);
            
            var nextPos = currentPos + vel;
            
            ProcessAutoScroll(ref nextPos, dt);
            ProcessMoveToPosition(ref nextPos);

            _scrollRect.content.anchoredPosition = nextPos;
        }

        private void ProcessStickToSide(ref Vector2 inputDelta) {
            if (!_enableStickToSide) {
                _stickDir = default;
                return;
            }
            
            float time = Time.realtimeSinceStartup;
            var currentPos = _scrollRect.normalizedPosition;
            var size = _scrollRect.content.rect.size;
            
            if (_stickDir.x >= 0f && size.x * currentPos.x > _sideWidth || _lastInputDir.x > 0f) _lastTimeNotTouchedSide.x = time;
            if (_stickDir.x <= 0f && size.x * (1f - currentPos.x) > _sideWidth || _lastInputDir.x < 0f) _lastTimeNotTouchedSide.y = time;
            if (_stickDir.y >= 0f && size.y * currentPos.y > _sideHeight || _lastInputDir.y > 0f) _lastTimeNotTouchedSide.z = time;
            if (_stickDir.y <= 0f && size.y * (1f - currentPos.y) > _sideHeight || _lastInputDir.y < 0f) _lastTimeNotTouchedSide.w = time;
            
            _stickDir = default;
            
            if ((StickMode.Right & _stickMode) == StickMode.Right &&
                time - _lastTimeNotTouchedSide.x > _stickStartDelay) 
            {
                _stickDir.x = -1f;
            }

            if ((StickMode.Left & _stickMode) == StickMode.Left &&
                time - _lastTimeNotTouchedSide.y > _stickStartDelay)
            {
                _stickDir.x = 1f;
            }

            if ((StickMode.Bottom & _stickMode) == StickMode.Bottom &&
                time - _lastTimeNotTouchedSide.z > _stickStartDelay)
            {
                _stickDir.y = -1f;
            }
                
            if ((StickMode.Top & _stickMode) == StickMode.Top &&
                time - _lastTimeNotTouchedSide.w > _stickStartDelay)
            {
                _stickDir.y = 1f;
            }

            inputDelta += _stickSpeed * _stickDir;
        }

        private void ProcessAutoScroll(ref Vector2 nextPos, float dt) {
            if (!_enableAutoScroll ||
                Time.realtimeSinceStartup - _lastTimeHasInputs < _autoscrollStartDelay) 
            {
                return;
            }

            var target = new Vector2(_autoscrollPositionX, 1f - _autoscrollPositionY) * _scrollRect.content.rect.size;
            nextPos = nextPos.SmoothExpNonZero(target, _autoscrollSmoothing, dt);
        }

        private void ProcessMoveToPosition(ref Vector2 nextPos) {
            float time = Time.realtimeSinceStartup;
            if (time - _moveToPositionStartTime > _moveToPositionDuration) return;

            float t = _moveToPositionDuration > 0f 
                ? Mathf.Clamp01((time - _moveToPositionStartTime) / _moveToPositionDuration) 
                : 1f;

            var target = _targetMovePositionNormalized * _scrollRect.content.rect.size;
            nextPos = Vector2.Lerp(_startMovePosition, target, _moveToPositionEasing.Evaluate(t));
            
            _lastTimeHasInputs = time;
            _lastTimeNotTouchedSide = time.ToVectorXYZW();
        }

        private bool IsFocused() {
            bool hasCursor = Cursor.visible && Cursor.lockState != CursorLockMode.Locked;
            return hasCursor && IsFocusedWithCursor() || !hasCursor && IsFocusedWithoutCursor();
        }

        private bool IsFocusedWithCursor() {
            bool insideScrollRect = UiNavigationUtils.IsCursorInsideRect(_scrollRect.viewport);
            
            return insideScrollRect && GetFrontScrollRectData().isFront || 
                   
                   !insideScrollRect && 
                   (_containsSelectedObjectDirectly || 
                   
                    GetFrontScrollRectData() is var data && 
                    (!data.hasOthers || !data.insideOthers && IsScrollbarSelected())
                   );
        }

        private bool IsFocusedWithoutCursor() {
            return _containsSelectedObjectDirectly || IsScrollbarSelected();
        }
        
        private (bool isFront, bool insideOthers, bool hasOthers) GetFrontScrollRectData() {
            bool insideOtherScrollRects = false;
            bool hasOthers = false;
            
            foreach (var viewport in _navigationService.ScrollableViewports) {
                if (viewport == _scrollRect.viewport) continue;

                hasOthers = true;
                
                if (!UiNavigationUtils.IsCursorInsideRect(viewport)) continue;
                
                insideOtherScrollRects = true;
                if (viewport.IsChildOf(_scrollRect.viewport)) return (isFront: false, insideOthers: true, hasOthers: true);
            }

            return (isFront: true, insideOtherScrollRects, hasOthers);
        }

        private bool ContainsSelectedObjectDirectly(IUiNavigationService navigationService) {
            if (_parentNode == null ||
                !navigationService.HasSelectedGameObject ||
                (navigationService.SelectedObjectOptions & UiNavigationOptions.Scrollable) == UiNavigationOptions.Scrollable && 
                !IsScrollbar(navigationService.CurrentSelectable)) 
            {
                return false;
            }
            
            var selectableParentNode = navigationService.GetParentNavigationNode(navigationService.CurrentSelectable);

            while (selectableParentNode != _parentNode) {
                if (selectableParentNode == null || selectableParentNode.IsScrollable) return false;

                selectableParentNode = navigationService.GetParentNavigationNode(selectableParentNode);
            }
            
            return true;
        }

        private bool IsScrollbarSelected() {
            return _navigationService.HasSelectedGameObject && IsScrollbar(_navigationService.CurrentSelectable);
        }
        
        private bool IsScrollbar(Selectable selectable) {
            return selectable == _scrollRect.horizontalScrollbar || selectable == _scrollRect.verticalScrollbar;
        }
        
        private Vector2 GetInputDelta() {
            Vector2 vectorMax = default;
            Vector2 deltaMax = default;

            for (int i = 0; i < _inputs.Length; i++) {
                ref var input = ref _inputs[i];
                var value = GetValue(ref input);

                switch (input.mode) {
                    case InputMode.Delta:
                        if (value.sqrMagnitude > deltaMax.sqrMagnitude) deltaMax = value;
                        break;
                    
                    case InputMode.Vector:
                        if (value.sqrMagnitude > vectorMax.sqrMagnitude) vectorMax = value;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            float accelerationMul = (_accelerateInput.Get()?.ReadValue<float>() ?? 0f) * _accelerationMul;
            deltaMax *= (1f + accelerationMul) * _deltaSensitivity;
            vectorMax *= (1f + accelerationMul) * _vectorSensitivity;
            
            return vectorMax == default ? deltaMax : vectorMax;
        }
        
        private static Vector2 GetValue(ref ScrollInput input) {
            var inputAction = input.inputAction.Get();
            var vector = inputAction.ReadValue<Vector2>();
            
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
            FetchEventHandlers();
        }

        [Button]
        private void FetchScrollbarHandlers() {
            if (FetchEventHandlers()) EditorUtility.SetDirty(this);
        }
        
        private bool FetchEventHandlers() {
            if (_scrollRect == null) return false;

            bool changed = false;
            int length = (_scrollRect.verticalScrollbar != null).AsInt() + 
                         (_scrollRect.horizontalScrollbar != null).AsInt();

            if (_pointerEventHandlers == null || _pointerEventHandlers.Length != length) {
                _pointerEventHandlers = new PointerEventsHandler[length];
                changed = true;
            }
            
            if (_scrollRect.verticalScrollbar != null) {
                var handler = _scrollRect.verticalScrollbar.gameObject.GetOrAddComponent<PointerEventsHandler>();

                changed |= _pointerEventHandlers[0] != handler;
                
                _pointerEventHandlers[0] = handler;
            }
            
            if (_scrollRect.horizontalScrollbar != null) {
                int index = _scrollRect.verticalScrollbar == null ? 0 : 1;
                var handler = _scrollRect.horizontalScrollbar.gameObject.GetOrAddComponent<PointerEventsHandler>();
                
                changed |= _pointerEventHandlers[index] != handler;
                
                _pointerEventHandlers[index] = handler;
            }

            return changed;
        }
#endif
    }
    
}