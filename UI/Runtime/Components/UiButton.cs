using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    [RequireComponent(typeof(Button))]
    public sealed class UiButton : 
        MonoBehaviour, IActorComponent,
        IPointerMoveHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler,
        ISubmitHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _buttonText;
        [EmbeddedInspector]
        [SerializeField] private UiButtonPreset _preset;

        [Header("Click")]
        [SerializeField] private ActionMode _actionMode = ActionMode.WaitPreviousAction;
        [SerializeField] private CancelMode _cancelMode = CancelMode.NonCancelable;
        [SerializeReference] [SubclassSelector] private IActorAction _clickAction;

        public event Action OnClicked = delegate { };
        public event Action<State> OnStateChanged = delegate { };
        public State CurrentState { get; private set; } = State.Default;

        public enum State {
            Default,
            Hover,
            Selected,
            Pressed,
        }

        private enum ActionMode {
            InvokeNewAction,
            CancelPreviousAction,
            WaitPreviousAction,
        } 
        
        private enum CancelMode {
            NonCancelable,
            OnButtonDisabled,
            OnButtonDestroyed,
        }

        private struct StateData {
            public float duration;
            public AnimationCurve curve;
            public Color imageColor;
            public Color textColor;
            public float scale;
        }
        
        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _clickCts;
        private IActor _actor;
        private bool _isHovering;
        private bool _isPressed;
        private Vector3 _originalScale;
        private byte _transitionId;
        
        private float _clickTime;
        private byte _clickActionId;
        private byte _awaitClickActionId;
        
        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);
            
            _originalScale = _button.transform.localScale;
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _button.onClick.AddListener(OnClick);

            ApplyState(GetState(), force: true);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            AsyncExt.DisposeCts(ref _clickCts);
            
            _isHovering = false;
            _isPressed = false;
            
            _button.onClick.RemoveListener(OnClick);
        }
        
        private void OnClick() {
            if (!CanClick()) return;

            _clickTime = Time.realtimeSinceStartup;

            if (_clickAction == null) {
                OnClicked.Invoke();
                return;
            }

            var cancellationToken = _cancelMode switch {
                CancelMode.NonCancelable => CancellationToken.None,
                CancelMode.OnButtonDisabled => _enableCts.Token,
                CancelMode.OnButtonDestroyed => _destroyCts.Token,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            switch (_actionMode) {
                case ActionMode.InvokeNewAction:
                    break;
                
                case ActionMode.CancelPreviousAction:
                    AsyncExt.RecreateCts(ref _clickCts);
                    cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _clickCts.Token).Token;
                    break;
                
                case ActionMode.WaitPreviousAction:
                    if (_clickActionId > _awaitClickActionId) return;

                    _clickActionId.IncrementUncheckedRef();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            OnClicked.Invoke();
            WaitClickAction(cancellationToken).Forget();
        }

        private async UniTask WaitClickAction(CancellationToken cancellationToken) {
            await _clickAction.Apply(_actor, cancellationToken);

            _awaitClickActionId = _clickActionId.IncrementUncheckedRef();
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData) {
            if (CanClick()) AnimateSubmit(_enableCts.Token).Forget();
        }
        
        void ISelectHandler.OnSelect(BaseEventData eventData) {
            OnSelect();
        }

        private void OnSelect() {
            _isHovering = true;
            
            var next = CurrentState == State.Pressed
                ? State.Pressed 
                : State.Selected;
            
            ApplyState(next);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData) {
            _isHovering = false;
            
            var next = CurrentState == State.Pressed
                ? State.Pressed 
                : State.Default;
            
            ApplyState(next);
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData) {
            _isHovering = true;
            
            var next = CurrentState == State.Pressed
                ? State.Pressed
                : IsSelected() ? State.Selected 
                : State.Hover;
            
            ApplyState(next);
            
            if (_preset.selectOnHover) _button.Select();
        }

        public void OnPointerExit(PointerEventData eventData) {
            _isHovering = false;
            
            var next = CurrentState == State.Pressed
                ? State.Pressed
                : IsSelected() ? State.Selected 
                : State.Default;
            
            ApplyState(next);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            _isPressed = true;
            
            var next = CanBePressed()
                ? State.Pressed
                : IsSelected() ? State.Selected
                : _isHovering ? State.Hover 
                : State.Default;

            ApplyState(next);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            _isPressed = false;
            
            var next = IsSelected() 
                ? State.Selected 
                : _isHovering ? State.Hover 
                : State.Default;
            
            ApplyState(next);
        }

        private void ApplyState(State state, bool force = false) {
            if (state == CurrentState && !force) return;
            
            CurrentState = state;
            OnStateChanged.Invoke(CurrentState);
            
            byte id = _transitionId.IncrementUncheckedRef();
            if (_enableCts != null) TransitTo(id, state, instant: force, _enableCts.Token).Forget();
        }

        private State GetState() {
            return _isPressed ? State.Pressed 
                : IsSelected() ? State.Selected 
                : _isHovering ? State.Hover 
                : State.Default;
        }
        
        private bool CanClick() {
            return Time.realtimeSinceStartup > _clickTime + _preset.clickCooldown && !IsAwaitingClickAction();
        }

        private bool IsAwaitingClickAction() {
            return _actionMode == ActionMode.WaitPreviousAction && _clickActionId != _awaitClickActionId;
        }
        
        private bool CanBePressed() {
            return _isPressed && _isHovering && IsSelected() && !IsAwaitingClickAction();
        }
        
        private bool IsSelected() {
            return EventSystem.current?.currentSelectedGameObject == _button.gameObject;
        }

        private async UniTask AnimateSubmit(CancellationToken cancellationToken) {
            byte id = _transitionId.IncrementUncheckedRef();
            
            await TransitTo(id, State.Pressed, instant: false, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested || id != _transitionId) return;
            
            await TransitTo(id, GetState(), instant: false, cancellationToken);
        }
        
        private async UniTask TransitTo(byte id, State state, bool instant, CancellationToken cancellationToken) {
            bool applyImageColor = _preset.applyColorToImage && _button.image != null;
            bool applyTextColor = _preset.applyColorToText && _buttonText != null;

            GetStateData(state, out var data);

            var trf = _button.transform;
            
            var startScale = trf.localScale;
            var endScale = _originalScale * data.scale;

            var startImageColor = applyImageColor ? _button.image.color : default;
            var startTextColor = applyTextColor ? _buttonText.color : default;
            
            float speed = instant || data.duration <= 0f ? float.MaxValue : 1f / data.duration;
            float t = 0f;
            
            while (!cancellationToken.IsCancellationRequested && id == _transitionId && t < 1f) {
                t = Mathf.Clamp01(t + speed * Time.unscaledDeltaTime);
                float p = data.curve.Evaluate(t);
                
                trf.localScale = Vector3.Lerp(startScale, endScale, p);

                if (applyImageColor) _button.image.color = Color.Lerp(startImageColor, data.imageColor, p);
                if (applyTextColor) _buttonText.color = Color.Lerp(startTextColor, data.textColor, p);
                
                await UniTask.Yield();
            }
        }

        private void GetStateData(State state, out StateData data) {
            data = default;
            
            switch (state) {
                case State.Default:
                    data.duration = _preset.defaultDuration;
                    data.curve = _preset.defaultCurve;
                    data.imageColor = _preset.defaultColorImage;
                    data.textColor = _preset.defaultColorText;
                    data.scale = _preset.defaultScale;
                    break;
                
                case State.Hover:
                    data.duration = _preset.hoverDuration;
                    data.curve = _preset.hoverCurve;
                    data.imageColor = _preset.hoverColorImage;
                    data.textColor = _preset.hoverColorText;
                    data.scale = _preset.hoverScale;
                    break;
                
                case State.Selected:
                    data.duration = _preset.selectDuration;
                    data.curve = _preset.selectCurve;
                    data.imageColor = _preset.selectColorImage;
                    data.textColor = _preset.selectColorText;
                    data.scale = _preset.selectScale;
                    break;
                
                case State.Pressed:
                    data.duration = _preset.pressDuration;
                    data.curve = _preset.pressCurve;
                    data.imageColor = _preset.pressColorImage;
                    data.textColor = _preset.pressColorText;
                    data.scale = _preset.pressScale;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _button = GetComponent<Button>();
            _buttonText = _button.GetComponentInChildren<TMP_Text>();
        }
#endif
    }
    
}