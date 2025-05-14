using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
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

        [Header("Click")]
        [SerializeField] private CancelMode _cancelMode;
        [SerializeReference] [SubclassSelector] private IActorAction _clickAction;
        
        [Header("Animation")]
        [SerializeField] private bool _selectOnHover = true;
        [SerializeField] [Min(0f)] private float _defaultDuration = 0.25f;
        [SerializeField] [Min(0f)] private float _hoverDuration = 0.25f;
        [SerializeField] [Min(0f)] private float _selectDuration = 0.25f;
        [SerializeField] [Min(0f)] private float _pressDuration = 0.25f;
        [SerializeField] private AnimationCurve _defaultCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeField] private AnimationCurve _hoverCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeField] private AnimationCurve _selectCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeField] private AnimationCurve _pressCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeField] private float _defaultScale = 1f;
        [SerializeField] private float _hoverScale = 1f;
        [SerializeField] private float _selectScale = 1f;
        [SerializeField] private float _pressScale = 1.1f;
        
        [Header("Colors")]
        [SerializeField] private bool _applyColorToImage;
        [VisibleIf(nameof(_applyColorToImage))]
        [SerializeField] private Color _defaultColorImage = Color.white;
        [VisibleIf(nameof(_applyColorToImage))]
        [SerializeField] private Color _hoverColorImage = Color.white;
        [VisibleIf(nameof(_applyColorToImage))]
        [SerializeField] private Color _selectColorImage = Color.white;
        [VisibleIf(nameof(_applyColorToImage))]
        [SerializeField] private Color _pressColorImage = Color.white;
        [SerializeField] private bool _applyColorToText;
        [VisibleIf(nameof(_applyColorToText))]
        [SerializeField] private Color _defaultColorText = Color.white;
        [VisibleIf(nameof(_applyColorToText))]
        [SerializeField] private Color _hoverColorText = Color.white;
        [VisibleIf(nameof(_applyColorToText))]
        [SerializeField] private Color _selectColorText = Color.white;
        [VisibleIf(nameof(_applyColorToText))]
        [SerializeField] private Color _pressColorText = Color.white;

        public event Action OnClicked = delegate { };
        public event Action<State> OnStateChanged = delegate { };
        public State CurrentState { get; private set; } = State.Default;

        public enum State {
            Default,
            Hover,
            Selected,
            Pressed,
        }

        private enum CancelMode {
            NotCancelable,
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
        
        private CancellationTokenSource _enableCts;
        private IActor _actor;
        private bool _isHovering;
        private bool _isPressed;
        private Vector3 _originalScale;
        private byte _transitionId;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void Awake() {
            _originalScale = _button.transform.localScale;
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _button.onClick.AddListener(OnClick);

            ApplyState(GetState(), force: true);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _isHovering = false;
            _isPressed = false;
            
            _button.onClick.RemoveListener(OnClick);
        }
        
        private void OnClick() {
            OnClicked.Invoke();
            
            if (_clickAction == null) return;

            var cancellationToken = _cancelMode switch {
                CancelMode.NotCancelable => CancellationToken.None,
                CancelMode.OnButtonDisabled => _enableCts.Token,
                CancelMode.OnButtonDestroyed => destroyCancellationToken,
                _ => throw new ArgumentOutOfRangeException()
            };

            _clickAction.Apply(_actor, cancellationToken).Forget();
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData) {
            AnimateSubmit(_enableCts.Token).Forget();
        }
        
        void ISelectHandler.OnSelect(BaseEventData eventData) {
            OnSelect();
        }

        private void OnSelect() {
            _isHovering = true;
            
            var next = _isPressed && CanBePressed()
                ? State.Pressed 
                : State.Selected;
            
            ApplyState(next);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData) {
            _isHovering = false;
            
            var next = _isPressed && CanBePressed()
                ? State.Pressed 
                : State.Default;
            
            ApplyState(next);
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData) {
            _isHovering = true;
            
            var next = _isPressed && CanBePressed()
                ? State.Pressed 
                : IsSelected() ? State.Selected : State.Hover;
            
            ApplyState(next);
            
            if (_selectOnHover) _button.Select();
        }

        public void OnPointerExit(PointerEventData eventData) {
            _isHovering = false;
            
            var next = _isPressed && CanBePressed()
                ? State.Pressed
                : IsSelected() ? State.Selected : State.Default;
            
            ApplyState(next);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            _isPressed = true;
            
            var next = _isPressed && CanBePressed()
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
                : _isHovering ? State.Hover : State.Default;
            
            ApplyState(next);
        }

        private void ApplyState(State state, bool force = false) {
            if (state == CurrentState && !force) return;

            CurrentState = state;
            OnStateChanged.Invoke(CurrentState);
            
            byte id = ++_transitionId;
            if (_enableCts != null) TransitTo(id, state, instant: force, _enableCts.Token).Forget();
        }

        private State GetState() {
            return _isPressed ? State.Pressed 
                : IsSelected() ? State.Selected 
                : _isHovering ? State.Hover 
                : State.Default;
        }

        private bool CanBePressed() {
            return _isHovering && IsSelected();
        }
        
        private bool IsSelected() {
            return EventSystem.current?.currentSelectedGameObject == _button.gameObject;
        }

        private async UniTask AnimateSubmit(CancellationToken cancellationToken) {
            byte id = ++_transitionId;
            
            await TransitTo(id, State.Pressed, instant: false, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested || id != _transitionId) return;
            
            await TransitTo(id, GetState(), instant: false, cancellationToken);
        }
        
        private async UniTask TransitTo(byte id, State state, bool instant, CancellationToken cancellationToken) {
            bool applyImageColor = _applyColorToImage && _button.image != null;
            bool applyTextColor = _applyColorToText && _buttonText != null;

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
                    data.duration = _defaultDuration;
                    data.curve = _defaultCurve;
                    data.imageColor = _defaultColorImage;
                    data.textColor = _defaultColorText;
                    data.scale = _defaultScale;
                    break;
                
                case State.Hover:
                    data.duration = _hoverDuration;
                    data.curve = _hoverCurve;
                    data.imageColor = _hoverColorImage;
                    data.textColor = _hoverColorText;
                    data.scale = _hoverScale;
                    break;
                
                case State.Selected:
                    data.duration = _selectDuration;
                    data.curve = _selectCurve;
                    data.imageColor = _selectColorImage;
                    data.textColor = _selectColorText;
                    data.scale = _selectScale;
                    break;
                
                case State.Pressed:
                    data.duration = _pressDuration;
                    data.curve = _pressCurve;
                    data.imageColor = _pressColorImage;
                    data.textColor = _pressColorText;
                    data.scale = _pressScale;
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