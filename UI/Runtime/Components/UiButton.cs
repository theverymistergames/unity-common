using System;
using System.Collections.Generic;
using System.Threading;
using MisterGames.Common.Async;
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
        
        public event Action OnClicked = delegate { };
        
        private readonly HashSet<int> _blocks = new();
        private CancellationTokenSource _enableCts;
        private IUiElementAnimator _uiElementAnimator;
        private float _clickTime;

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
            if (IsBlocked()) {
                _uiElementAnimator?.ApplyCustomState(UiElementState.Blocked);
                _button.interactable = false;
                return;
            }

            _button.interactable = true;
            _uiElementAnimator?.ResetCustomState();
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

            _clickTime = Time.realtimeSinceStartup;
            OnClicked.Invoke();
        }

        private bool IsBlocked() {
            return _blocks.Count > 0;
        }

        private bool CanClick() {
            return !IsBlocked() && Time.realtimeSinceStartup > _clickTime + _clickCooldown;
        }
        
#if UNITY_EDITOR
        [SerializeField] private bool _isBlockedDebug;
        
        private void Reset() {
            _button = GetComponent<Button>();
        }

        private void OnValidate() {
            if (!Application.isPlaying) return;
            
            //Block(this, _isBlockedDebug);
        }
#endif
    }
    
}