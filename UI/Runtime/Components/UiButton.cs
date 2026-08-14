using System;
using System.Collections.Generic;
using System.Threading;
using MisterGames.Common.Async;
using MisterGames.Common.Tick;
using MisterGames.UI.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    [RequireComponent(typeof(Button))]
    public sealed class UiButton : MonoBehaviour, ISubmitHandler, IUiElementAnimated {
        
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] [Min(0f)] private float _clickCooldown = 0.1f;
        [SerializeField] private bool _isBlocked;
        
        public event Action<UiButton> OnClicked = delegate { };
        public Selectable Selectable => _button;
        public TMP_Text ButtonText => _buttonText;
        
        private readonly HashSet<int> _blocks = new();
        private CancellationTokenSource _enableCts;
        private IUiElementAnimator _uiElementAnimator;
        private float _clickTime;

        private void Awake() {
            Block(this, _isBlocked);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _button.onClick.AddListener(OnClick);
            
            CheckBlockState();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _button.onClick.RemoveListener(OnClick);
        }

        public void ClickManual() {
            if (!CanClick()) return;
            
            OnClick();
            _uiElementAnimator?.AnimateState(UiElementState.Pressed);
        }

        public void Block(object source, bool block) {
            if (block) _blocks.Add(source.GetHashCode());
            else _blocks.Remove(source.GetHashCode());
            
            CheckBlockState();
        }

        private void CheckBlockState() {
            _uiElementAnimator?.SetBlockedState(IsBlocked());
        }
        
        void ISubmitHandler.OnSubmit(BaseEventData eventData) {
            if (IsBlocked()) return;
            
            _uiElementAnimator?.AnimateState(UiElementState.Pressed);
        }

        void IUiElementAnimated.BindAnimator(IUiElementAnimator animator) {
            _uiElementAnimator = animator;
            CheckBlockState();
        }

        private void OnClick() {
            if (!CanClick()) return;

            _clickTime = TimeSources.unscaledTime;
            OnClicked.Invoke(this);
        }

        private bool IsBlocked() {
            return _blocks.Count > 0;
        }

        private bool CanClick() {
            return !IsBlocked() && TimeSources.unscaledTime > _clickTime + _clickCooldown;
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _button = GetComponent<Button>();
        }

        private void OnValidate() {
            if (!Application.isPlaying) return;
            
            Block(this, _isBlocked);
        }
#endif
    }
    
}