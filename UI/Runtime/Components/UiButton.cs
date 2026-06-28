using System;
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
        
        private CancellationTokenSource _enableCts;
        private IUiElementAnimator _uiElementAnimator;
        private float _clickTime;

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            _button.onClick.RemoveListener(OnClick);
        }

        public void ClickManual() {
            OnClick();
            _uiElementAnimator?.AnimateState(UiElementState.Pressed);
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData) {
            _uiElementAnimator?.AnimateState(UiElementState.Pressed);
        }

        void IUiElementAnimated.BindAnimator(IUiElementAnimator animator) {
            _uiElementAnimator = animator;
        }

        private void OnClick() {
            if (!CanClick()) return;

            _clickTime = Time.realtimeSinceStartup;
            OnClicked.Invoke();
        }

        private bool CanClick() {
            return Time.realtimeSinceStartup > _clickTime + _clickCooldown;
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _button = GetComponent<Button>();
        }
#endif
    }
    
}