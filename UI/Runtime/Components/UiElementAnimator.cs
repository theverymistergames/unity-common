using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using MisterGames.UI.Data;
using MisterGames.UI.Navigation;
using MisterGames.UI.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public sealed class UiElementAnimator : MonoBehaviour, IUiElementAnimator,
        IPointerMoveHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Transform _elementRoot;
        [SerializeField] private Selectable _selectable;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _image;
        [EmbeddedInspector]
        [SerializeField] private UiElementPreset _preset;

        public event Action<UiElementState> OnStateChanged = delegate { };
        public UiElementState CurrentState { get; private set; } = UiElementState.Default;

        private CancellationTokenSource _enableCts;
        private bool _isHovering;
        private bool _isPressed;
        private bool _isBlocked;
        private Vector3 _originalScale;
        private byte _transitionId;

        private void Awake() {
            _originalScale = _elementRoot.localScale;

            if (_elementRoot.TryGetComponent(out IUiElementAnimated element)) {
                element.BindAnimator(this);
            }
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);

            if (Services.TryGet(out IUiNavigationService service)) { 
                service.OnSelectableChanged += OnSelectableChanged;    
            }
            
            ApplyState(GetAutoState(), force: true);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            if (Services.TryGet(out IUiNavigationService service)) { 
                service.OnSelectableChanged -= OnSelectableChanged;    
            }
            
            _isHovering = false;
            _isPressed = false;
        }

        private void OnSelectableChanged(Selectable selectable, IUiWindow window) {
            if (selectable == _selectable) OnSelect();
            else OnDeselect();
        }

        void ISelectHandler.OnSelect(BaseEventData eventData) {
            OnSelect();
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData) {
            OnDeselect();
        }

        private void OnSelect() {
            _isHovering = true;
            
            var next = CurrentState == UiElementState.Pressed
                ? UiElementState.Pressed 
                : UiElementState.Selected;

            if (_isBlocked) next = GetAutoState();
            
            ApplyState(next);
        }

        private void OnDeselect() {
            _isHovering = false;
            
            var next = CurrentState == UiElementState.Pressed
                ? UiElementState.Pressed 
                : UiElementState.Default;
            
            if (_isBlocked) next = GetAutoState();
            
            ApplyState(next);
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData) {
            _isHovering = true;
            
            var next = CurrentState == UiElementState.Pressed
                ? UiElementState.Pressed
                : IsSelected() ? UiElementState.Selected 
                    : UiElementState.Hover;
            
            if (_isBlocked) next = GetAutoState();
            
            ApplyState(next);
            
            if (_preset.selectOnHover && _selectable != null) _selectable.Select();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
            _isHovering = false;
            
            var next = CurrentState == UiElementState.Pressed
                ? UiElementState.Pressed
                : IsSelected() ? UiElementState.Selected 
                    : UiElementState.Default;
            
            if (_isBlocked) next = GetAutoState();
            
            ApplyState(next);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            if (_isBlocked) return;
            
            _isPressed = true;
            
            var next = CanBePressed()
                ? UiElementState.Pressed
                : IsSelected() ? UiElementState.Selected
                    : _isHovering ? UiElementState.Hover 
                        : UiElementState.Default;
            
            ApplyState(next);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            if (_isBlocked) return;
            
            _isPressed = false;

            var next = IsSelected() 
                ? UiElementState.Selected 
                : _isHovering ? UiElementState.Hover 
                    : UiElementState.Default;
            
            ApplyState(next);
        }

        void IUiElementAnimator.SetBlockedState(bool blocked) {
            _isBlocked = blocked;
            
            ApplyState(GetAutoState());
        }

        void IUiElementAnimator.AnimateState(UiElementState state) {
            if (_enableCts != null) AnimateStateAsync(state, _enableCts.Token).Forget();
        }

        private void ApplyState(UiElementState state, bool force = false) {
            if (state == CurrentState && !force) return;
            
            CurrentState = state;
            OnStateChanged.Invoke(CurrentState);
            
            if (_enableCts == null) return;
            
            byte id = _transitionId.IncrementUncheckedRef();
            _preset.GetStateData(state, out var data);
            AnimateTransitionTo(id, data, force, _enableCts.Token).Forget();
        }

        private async UniTask AnimateStateAsync(UiElementState state, CancellationToken cancellationToken) {
            byte id = _transitionId.IncrementUncheckedRef();
            
            _preset.GetStateData(state, out var data);
            await AnimateTransitionTo(id, data, instant: false, cancellationToken);
            
            if (id != _transitionId || cancellationToken.IsCancellationRequested) return;
            
            _preset.GetStateData(GetAutoState(), out data);
            await AnimateTransitionTo(id, data, instant: false, cancellationToken);
        }

        private async UniTask AnimateTransitionTo(byte id, UiElementStateData data, bool instant, CancellationToken cancellationToken) {
            bool applyImageColor = _preset.applyColorToImage && _image != null;
            bool applyTextColor = _preset.applyColorToText && _text != null;

            var trf = _elementRoot;
            
            var startScale = trf.localScale;
            var endScale = _originalScale * data.scale;

            var startImageColor = applyImageColor ? _image.color : default;
            var startTextColor = applyTextColor ? _text.color : default;
            
            float speed = instant || data.duration <= 0f ? float.MaxValue : 1f / data.duration;
            float t = 0f;
            
            while (!cancellationToken.IsCancellationRequested && id == _transitionId && t < 1f) {
                t = Mathf.Clamp01(t + speed * Time.unscaledDeltaTime);
                float p = data.curve.Evaluate(t);
                
                trf.localScale = Vector3.Lerp(startScale, endScale, p);

                if (applyImageColor) _image.color = Color.Lerp(startImageColor, data.imageColor, p);
                if (applyTextColor) _text.color = Color.Lerp(startTextColor, data.textColor, p);
                
                await UniTask.Yield();
            }
        }
        
        private UiElementState GetAutoState() {
            if (_isBlocked) {
                return IsSelected() ? UiElementState.BlockedSelected : UiElementState.Blocked;
            }
            
            return _isPressed ? UiElementState.Pressed 
                : IsSelected() ? UiElementState.Selected 
                : _isHovering ? UiElementState.Hover 
                : UiElementState.Default;
        }

        private bool CanBePressed() {
            return _isPressed && _isHovering && !_isBlocked && IsSelected();
        }

        private bool IsSelected() {
            return _selectable != null && 
                   (Services.Get<UiNavigationService>()?.LastNonNullSelectable == _selectable || 
                    EventSystem.current?.currentSelectedGameObject == _selectable.gameObject);
        }

#if UNITY_EDITOR
        private void Reset() {
            _selectable = GetComponent<Selectable>();
            _elementRoot = transform;
            _text = GetComponentInChildren<TMP_Text>();
            _image = GetComponentInChildren<Image>();
        }
#endif
    }
    
}